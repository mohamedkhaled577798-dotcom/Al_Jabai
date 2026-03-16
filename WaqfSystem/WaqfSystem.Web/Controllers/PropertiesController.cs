using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Web.ViewModels.Properties;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class PropertiesController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IDocumentService _documentService;
        private readonly IDqsService _dqsService;
        private readonly IGisSyncService _gisSyncService;
        private readonly ILogger<PropertiesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        // Helper: get numeric user ID from claims
        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        public PropertiesController(
            IPropertyService propertyService,
            IDocumentService documentService,
            IDqsService dqsService,
            IGisSyncService gisSyncService,
            ILogger<PropertiesController> logger,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _propertyService = propertyService;
            _documentService = documentService;
            _dqsService = dqsService;
            _gisSyncService = gisSyncService;
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        // ─────────────────────────────────────────────────────────────────────
        // INDEX
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(PropertyFilterRequest filter)
        {
            try
            {
                var results = await _propertyService.GetPagedAsync(filter, CurrentUserId);
                var governorates = await _unitOfWork.GetQueryable<Governorate>().ToListAsync();

                var vm = new PropertyIndexWrapperViewModel
                {
                    Results = results,
                    Filter = filter,
                    Governorates = new SelectList(governorates, "Id", "NameAr", filter.GovernorateId)
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل قائمة العقارات");
                return View(new PropertyIndexWrapperViewModel { Filter = filter });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE — STEP 1 GET
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var draftKey = Guid.NewGuid().ToString("N")[..12];
            var vm = new PropertyCreateViewModel
            {
                CurrentStep = 1,
                DraftKey = draftKey
            };
            vm.BasicInfo.PropertyTypeOptions = await BuildPropertyTypeSelectList();
            SaveDraft(draftKey, vm);
            return View("CreateStep1", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var detail = await _propertyService.GetByIdAsync((int)id);
            if (detail == null) return NotFound();

            var draftKey = Guid.NewGuid().ToString("N")[..12];
            var vm = MapDetailToDraft(detail);
            vm.DraftKey = draftKey;
            vm.CurrentStep = 1;
            vm.BasicInfo.PropertyTypeOptions = await BuildPropertyTypeSelectList();

            if (vm.Geographic.StreetId.HasValue)
            {
                var street = await _unitOfWork.GetQueryable<Street>()
                    .Include(s => s.Neighborhood)
                        .ThenInclude(n => n.SubDistrict)
                            .ThenInclude(sd => sd.District)
                    .FirstOrDefaultAsync(s => s.Id == vm.Geographic.StreetId.Value);

                if (street != null)
                {
                    vm.Geographic.NeighborhoodId = street.NeighborhoodId;
                    vm.Geographic.SubDistrictId = street.Neighborhood?.SubDistrictId ?? 0;
                    vm.Geographic.DistrictId = street.Neighborhood?.SubDistrict?.DistrictId ?? 0;
                    vm.Geographic.GovernorateId = detail.GovernorateId
                        ?? street.Neighborhood?.SubDistrict?.District?.GovernorateId
                        ?? vm.Geographic.GovernorateId;
                }
            }

            SaveDraft(draftKey, vm);
            SaveEditId(draftKey, (int)id);

            return View("CreateStep1", vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 1 POST
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep1(PropertyCreateViewModel vm)
        {
            ClearValidationExcept(ModelState, "BasicInfo");

            if (vm.BasicInfo.OwnershipType == "PARTNERSHIP" && vm.BasicInfo.WaqfSharePercent == null)
                ModelState.AddModelError("BasicInfo.WaqfSharePercent", "نسبة الوقف مطلوبة لنوع الملكية الشراكة");

            if (vm.BasicInfo.OwnershipType == "AGRICULTURAL" && vm.BasicInfo.TotalAreaDunum == null)
                ModelState.AddModelError("BasicInfo.TotalAreaDunum", "المساحة الكلية بالدونم مطلوبة للأراضي الزراعية");

            if (!ModelState.IsValid)
            {
                vm.BasicInfo.PropertyTypeOptions = await BuildPropertyTypeSelectList();
                vm.CurrentStep = 1;
                return View("CreateStep1", vm);
            }

            var draft = LoadDraft(vm.DraftKey!) ?? new PropertyCreateViewModel { DraftKey = vm.DraftKey };
            draft.BasicInfo = vm.BasicInfo;
            draft.CurrentStep = 2;
            SaveDraft(vm.DraftKey!, draft);

            return RedirectToAction("CreateStep2", new { draftKey = vm.DraftKey });
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 2 GET
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CreateStep2(string draftKey)
        {
            var draft = LoadDraft(draftKey);
            if (draft == null) return RedirectToAction("Create");

            draft.CurrentStep = 2;
            draft.DraftKey = draftKey;

            draft.Geographic.Governorates = await BuildGovernorateSelectList();

            if (draft.Geographic.GovernorateId > 0)
                draft.Geographic.Districts = await BuildDistrictSelectList(draft.Geographic.GovernorateId);

            if (draft.Geographic.DistrictId > 0)
                draft.Geographic.SubDistricts = await BuildSubDistrictSelectList(draft.Geographic.DistrictId);

            if (draft.Geographic.SubDistrictId > 0)
                draft.Geographic.Neighborhoods = await BuildNeighborhoodSelectList(draft.Geographic.SubDistrictId);

            if (draft.Geographic.NeighborhoodId.HasValue && draft.Geographic.NeighborhoodId.Value > 0)
                draft.Geographic.Streets = await BuildStreetSelectList(draft.Geographic.NeighborhoodId.Value);

            draft.DqsCriteria = ComputeDqsCriteria(draft);
            draft.DqsScore = draft.DqsCriteria.Sum(c => c.Score);

            SaveDraft(draftKey, draft);
            return View("CreateStep2", draft);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 2 POST
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep2(PropertyCreateViewModel vm)
        {
            ClearValidationExcept(ModelState, "Geographic");

            if (!ModelState.IsValid)
            {
                vm.CurrentStep = 2;
                vm.Geographic.Governorates = await BuildGovernorateSelectList();
                if (vm.Geographic.GovernorateId > 0)
                    vm.Geographic.Districts = await BuildDistrictSelectList(vm.Geographic.GovernorateId);
                if (vm.Geographic.DistrictId > 0)
                    vm.Geographic.SubDistricts = await BuildSubDistrictSelectList(vm.Geographic.DistrictId);
                if (vm.Geographic.SubDistrictId > 0)
                    vm.Geographic.Neighborhoods = await BuildNeighborhoodSelectList(vm.Geographic.SubDistrictId);
                if (vm.Geographic.NeighborhoodId.HasValue && vm.Geographic.NeighborhoodId.Value > 0)
                    vm.Geographic.Streets = await BuildStreetSelectList(vm.Geographic.NeighborhoodId.Value);
                return View("CreateStep2", vm);
            }

            var draft = LoadDraft(vm.DraftKey!) ?? new PropertyCreateViewModel { DraftKey = vm.DraftKey };
            draft.Geographic = vm.Geographic;
            draft.CurrentStep = 3;
            SaveDraft(vm.DraftKey!, draft);

            return RedirectToAction("CreateStep3", new { draftKey = vm.DraftKey });
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 3 GET
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult CreateStep3(string draftKey)
        {
            var draft = LoadDraft(draftKey);
            if (draft == null) return RedirectToAction("Create");

            draft.CurrentStep = 3;
            draft.DraftKey = draftKey;
            draft.MapLocation.GisBaseUrl = GetGisBaseUrl();
            draft.MapLocation.DefaultMapCenter = GetDefaultCenter(draft.Geographic.GovernorateId);
            draft.MapLocation.DefaultZoom = 13;

            draft.DqsCriteria = ComputeDqsCriteria(draft);
            draft.DqsScore = draft.DqsCriteria.Sum(c => c.Score);

            SaveDraft(draftKey, draft);
            return View("CreateStep3", draft);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 3 POST
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStep3(PropertyCreateViewModel vm)
        {
            ClearValidationExcept(ModelState, "MapLocation");

            if (!string.IsNullOrWhiteSpace(vm.MapLocation.GisPolygon))
            {
                if (!IsValidGeoJsonPolygon(vm.MapLocation.GisPolygon))
                    ModelState.AddModelError("MapLocation.GisPolygon", "صيغة GeoJSON غير صحيحة. يجب أن يكون مضلعاً بـ 4 نقاط على الأقل");
            }

            if (vm.MapLocation.Latitude.HasValue && vm.MapLocation.Longitude.HasValue)
            {
                if (!IsWithinIraq(vm.MapLocation.Latitude.Value, vm.MapLocation.Longitude.Value))
                    ModelState.AddModelError("MapLocation.Latitude", "الإحداثيات يجب أن تكون داخل حدود جمهورية العراق");
            }

            if (!ModelState.IsValid)
            {
                vm.CurrentStep = 3;
                return View("CreateStep3", vm);
            }

            var draft = LoadDraft(vm.DraftKey!) ?? new PropertyCreateViewModel { DraftKey = vm.DraftKey };
            draft.MapLocation = vm.MapLocation;
            draft.CurrentStep = 4;
            SaveDraft(vm.DraftKey!, draft);

            return RedirectToAction("CreateStep4", new { draftKey = vm.DraftKey });
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 4 GET
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult CreateStep4(string draftKey)
        {
            var draft = LoadDraft(draftKey);
            if (draft == null) return RedirectToAction("Create");

            draft.CurrentStep = 4;
            draft.DraftKey = draftKey;

            if (!draft.BuildingDetails.Floors.Any())
            {
                draft.BuildingDetails.Floors = GenerateDefaultFloors(
                    draft.BuildingDetails.FloorCount,
                    draft.BuildingDetails.BasementCount);
            }

            draft.DqsCriteria = ComputeDqsCriteria(draft);
            draft.DqsScore = draft.DqsCriteria.Sum(c => c.Score);

            SaveDraft(draftKey, draft);
            return View("CreateStep4", draft);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 4 POST
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStep4(PropertyCreateViewModel vm)
        {
            ClearValidationExcept(ModelState, "BuildingDetails");

            if (!vm.BuildingDetails.Floors.Any())
                ModelState.AddModelError("BuildingDetails.Floors", "يجب إضافة طابق واحد على الأقل");

            if (!ModelState.IsValid)
            {
                vm.CurrentStep = 4;
                return View("CreateStep4", vm);
            }

            var draft = LoadDraft(vm.DraftKey!) ?? new PropertyCreateViewModel { DraftKey = vm.DraftKey };
            draft.BuildingDetails = vm.BuildingDetails;
            draft.CurrentStep = 5;
            SaveDraft(vm.DraftKey!, draft);

            return RedirectToAction("CreateStep5", new { draftKey = vm.DraftKey });
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 5 GET
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult CreateStep5(string draftKey)
        {
            var draft = LoadDraft(draftKey);
            if (draft == null) return RedirectToAction("Create");

            draft.CurrentStep = 5;
            draft.DraftKey = draftKey;
            draft.DqsCriteria = ComputeDqsCriteria(draft);
            draft.DqsScore = draft.DqsCriteria.Sum(c => c.Score);

            SaveDraft(draftKey, draft);
            return View("CreateStep5", draft);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE STEP 5 POST
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> CreateStep5(PropertyCreateViewModel vm)
        {
            ClearValidationExcept(ModelState, "Documents");

            var allowedMimes = new[] { "application/pdf", "image/jpeg", "image/png", "image/tiff" };
            const long maxFileSize = 10L * 1024 * 1024; // 10 MB

            var allFiles = new (IFormFile? File, string FieldName)[]
            {
                (vm.Documents.DeedFile, "Documents.DeedFile"),
                (vm.Documents.CadastralFile, "Documents.CadastralFile"),
                (vm.Documents.BuildingPermitFile, "Documents.BuildingPermitFile"),
                (vm.Documents.CompletionCertFile, "Documents.CompletionCertFile"),
                (vm.Documents.PhotoFront, "Documents.PhotoFront"),
                (vm.Documents.PhotoRight, "Documents.PhotoRight"),
                (vm.Documents.PhotoLeft, "Documents.PhotoLeft"),
                (vm.Documents.PhotoBack, "Documents.PhotoBack"),
                (vm.Documents.PhotoInside, "Documents.PhotoInside")
            };

            foreach (var (file, fieldName) in allFiles)
            {
                if (file == null) continue;
                if (file.Length > maxFileSize)
                    ModelState.AddModelError(fieldName, $"حجم الملف '{file.FileName}' يتجاوز الحد الأقصى 10 ميغابايت");
                if (!allowedMimes.Contains(file.ContentType.ToLower()))
                    ModelState.AddModelError(fieldName, $"نوع الملف '{file.ContentType}' غير مقبول. المقبول: PDF, JPG, PNG, TIFF");
            }

            // Merge doc metadata into draft for DQS computation
            var draft = LoadDraft(vm.DraftKey!) ?? new PropertyCreateViewModel { DraftKey = vm.DraftKey };
            draft.Documents.DeedNumber = vm.Documents.DeedNumber;
            draft.Documents.DeedDate = vm.Documents.DeedDate;
            draft.Documents.DeedCourt = vm.Documents.DeedCourt;
            draft.Documents.DeedUploaded = draft.Documents.DeedUploaded || vm.Documents.DeedFile != null;
            draft.Documents.CadastralNumber = vm.Documents.CadastralNumber;
            draft.Documents.TabuNumber = vm.Documents.TabuNumber;
            draft.Documents.BuildingPermitNumber = vm.Documents.BuildingPermitNumber;
            draft.Documents.BuildingPermitDate = vm.Documents.BuildingPermitDate;
            draft.Documents.CompletionCertNumber = vm.Documents.CompletionCertNumber;
            draft.Documents.ExistingPhotoCount = vm.Documents.ExistingPhotoCount;

            draft.DqsCriteria = ComputeDqsCriteria(draft);
            draft.DqsScore = draft.DqsCriteria.Sum(c => c.Score);

            if (!ModelState.IsValid)
            {
                vm.CurrentStep = 5;
                vm.DqsCriteria = draft.DqsCriteria;
                vm.DqsScore = draft.DqsScore;
                return View("CreateStep5", vm);
            }

            if (draft.DqsScore < 50)
            {
                ModelState.AddModelError("", $"مؤشر جودة البيانات ({draft.DqsScore}%) أقل من الحد الأدنى المطلوب (50%). يرجى اكتمال البيانات.");
                vm.CurrentStep = 5;
                vm.DqsCriteria = draft.DqsCriteria;
                vm.DqsScore = draft.DqsScore;
                return View("CreateStep5", vm);
            }

            try
            {
                var editId = LoadEditId(vm.DraftKey!);
                int propertyId;

                if (editId.HasValue)
                {
                    var dto = MapDraftToUpdateDto(draft, editId.Value);
                    var result = await _propertyService.UpdateAsync(dto, CurrentUserId);
                    propertyId = result.Id;
                }
                else
                {
                    var dto = MapDraftToCreateDto(draft);
                    var result = await _propertyService.CreateAsync(dto, CurrentUserId);
                    propertyId = result.Id;
                }

                await UploadDocumentsAsync(propertyId, vm.Documents);

                if (draft.MapLocation.HasCoordinates)
                {
                    _ = Task.Run(async () =>
                    {
                        try { await _gisSyncService.SyncPropertyToGisAsync(propertyId); }
                        catch (Exception ex) { _logger.LogError(ex, "فشل مزامنة GIS للعقار {PropertyId}", propertyId); }
                    });
                }

                ClearDraft(vm.DraftKey!);
                ClearEditId(vm.DraftKey!);
                TempData["SuccessMessage"] = editId.HasValue
                    ? $"تم تعديل البناية بنجاح ✓ — رقم العقار: WQF-{propertyId:D6}"
                    : $"تم حفظ البناية بنجاح ✓ — رقم العقار: WQF-{propertyId:D6}";
                return RedirectToAction("Details", new { id = propertyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حفظ العقار");
                var msg = ex.InnerException != null ? $"{ex.Message} | تفاصيل: {ex.InnerException.Message}" : ex.Message;
                ModelState.AddModelError("", $"حدث خطأ أثناء الحفظ: {msg}");
                
                vm.CurrentStep = 5;
                vm.DqsCriteria = draft.DqsCriteria;
                vm.DqsScore = draft.DqsScore;
                return View("CreateStep5", vm);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DETAILS
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var detail = await _propertyService.GetByIdAsync((int)id);
            if (detail == null) return NotFound();
            return View(detail);
        }

        // ─────────────────────────────────────────────────────────────────────
        // AJAX — GEOGRAPHIC CASCADES
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetDistricts(int governorateId)
        {
            var items = await _unitOfWork.GetQueryable<District>()
                .Where(d => d.GovernorateId == governorateId)
                .OrderBy(d => d.NameAr)
                .ToListAsync();
            return Json(items.Select(d => new { value = d.Id, text = d.NameAr }));
        }

        [HttpGet]
        public async Task<IActionResult> GetSubDistricts(int districtId)
        {
            var items = await _unitOfWork.GetQueryable<SubDistrict>()
                .Where(d => d.DistrictId == districtId)
                .OrderBy(d => d.NameAr)
                .ToListAsync();
            return Json(items.Select(d => new { value = d.Id, text = d.NameAr }));
        }

        [HttpGet]
        public async Task<IActionResult> GetNeighborhoods(int subDistrictId)
        {
            var items = await _unitOfWork.GetQueryable<Neighborhood>()
                .Where(d => d.SubDistrictId == subDistrictId)
                .OrderBy(d => d.NameAr)
                .ToListAsync();
            return Json(items.Select(d => new { value = d.Id, text = d.NameAr }));
        }

        [HttpGet]
        public async Task<IActionResult> GetStreets(int neighborhoodId)
        {
            var items = await _unitOfWork.GetQueryable<Street>()
                .Where(d => d.NeighborhoodId == neighborhoodId)
                .OrderBy(d => d.NameAr)
                .ToListAsync();
            return Json(items.Select(d => new { value = d.Id, text = d.NameAr }));
        }

        // ─────────────────────────────────────────────────────────────────────
        // AJAX — DQS
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult ComputeDqs([FromBody] PropertyCreateViewModel vm)
        {
            var criteria = ComputeDqsCriteria(vm);
            var score = criteria.Sum(c => c.Score);
            return Json(new
            {
                score,
                criteria = criteria.Select(c => new { c.Label, c.Weight, c.Achieved, c.Score })
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // AJAX — SAVE POLYGON
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult SavePolygon([FromBody] SavePolygonRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.GeoJson) || !IsValidGeoJsonPolygon(req.GeoJson))
                return BadRequest(new { success = false, message = "صيغة GeoJSON غير صحيحة" });

            var draft = LoadDraft(req.DraftKey);
            if (draft == null)
                return NotFound(new { success = false, message = "المسودة غير موجودة" });

            draft.MapLocation.GisPolygon = req.GeoJson;
            draft.MapLocation.CalculatedAreaSqm = req.AreaSqm;
            SaveDraft(req.DraftKey, draft);

            return Ok(new { success = true, areaSqm = req.AreaSqm });
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — DRAFT HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private void SaveDraft(string key, PropertyCreateViewModel vm)
        {
            var json = JsonSerializer.Serialize(vm, new JsonSerializerOptions { WriteIndented = false });
            HttpContext.Session.SetString($"PropertyDraft_{key}", json);
        }

        private PropertyCreateViewModel? LoadDraft(string key)
        {
            var json = HttpContext.Session.GetString($"PropertyDraft_{key}");
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonSerializer.Deserialize<PropertyCreateViewModel>(json); }
            catch (Exception ex) { _logger.LogWarning(ex, "فشل تحميل المسودة للمفتاح {Key}", key); return null; }
        }

        private void ClearDraft(string key) => HttpContext.Session.Remove($"PropertyDraft_{key}");

        private void SaveEditId(string key, int propertyId)
            => HttpContext.Session.SetInt32($"PropertyEdit_{key}", propertyId);

        private int? LoadEditId(string key)
            => HttpContext.Session.GetInt32($"PropertyEdit_{key}");

        private void ClearEditId(string key)
            => HttpContext.Session.Remove($"PropertyEdit_{key}");

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — MODEL STATE FILTER
        // ─────────────────────────────────────────────────────────────────────
        private static void ClearValidationExcept(ModelStateDictionary modelState, params string[] prefixes)
        {
            var keysToRemove = modelState.Keys
                .Where(k => !prefixes.Any(p => k.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            foreach (var key in keysToRemove)
                modelState.Remove(key);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — SELECT LIST BUILDERS (using IUnitOfWork directly)
        // ─────────────────────────────────────────────────────────────────────
        private async Task<List<SelectListItem>> BuildGovernorateSelectList()
        {
            var govs = await _unitOfWork.GetQueryable<Governorate>().OrderBy(g => g.NameAr).ToListAsync();
            var list = new List<SelectListItem> { new SelectListItem("-- اختر المحافظة --", "0") };
            list.AddRange(govs.Select(g => new SelectListItem(g.NameAr, g.Id.ToString())));
            return list;
        }

        private async Task<List<SelectListItem>> BuildDistrictSelectList(int governorateId)
        {
            var items = await _unitOfWork.GetQueryable<District>()
                .Where(d => d.GovernorateId == governorateId).OrderBy(d => d.NameAr).ToListAsync();
            var list = new List<SelectListItem> { new SelectListItem("-- اختر القضاء --", "0") };
            list.AddRange(items.Select(i => new SelectListItem(i.NameAr, i.Id.ToString())));
            return list;
        }

        private async Task<List<SelectListItem>> BuildSubDistrictSelectList(int districtId)
        {
            var items = await _unitOfWork.GetQueryable<SubDistrict>()
                .Where(d => d.DistrictId == districtId).OrderBy(d => d.NameAr).ToListAsync();
            var list = new List<SelectListItem> { new SelectListItem("-- اختر الناحية --", "0") };
            list.AddRange(items.Select(i => new SelectListItem(i.NameAr, i.Id.ToString())));
            return list;
        }

        private async Task<List<SelectListItem>> BuildNeighborhoodSelectList(int subDistrictId)
        {
            var items = await _unitOfWork.GetQueryable<Neighborhood>()
                .Where(d => d.SubDistrictId == subDistrictId).OrderBy(d => d.NameAr).ToListAsync();
            var list = new List<SelectListItem> { new SelectListItem("-- اختر الحي --", "") };
            list.AddRange(items.Select(i => new SelectListItem(i.NameAr, i.Id.ToString())));
            return list;
        }

        private async Task<List<SelectListItem>> BuildStreetSelectList(int neighborhoodId)
        {
            var items = await _unitOfWork.GetQueryable<Street>()
                .Where(d => d.NeighborhoodId == neighborhoodId).OrderBy(d => d.NameAr).ToListAsync();
            var list = new List<SelectListItem> { new SelectListItem("-- اختر الشارع --", "") };
            list.AddRange(items.Select(i => new SelectListItem(i.NameAr, i.Id.ToString())));
            return list;
        }

        private async Task<List<SelectListItem>> BuildPropertyTypeSelectList()
        {
            // Use PropertyType enum since no separate PropertyType entity is expected
            var list = new List<SelectListItem> { new SelectListItem("-- اختر تصنيف العقار --", "0") };
            list.AddRange(Enum.GetValues<PropertyType>().Select(e =>
                new SelectListItem(GetPropertyTypeAr(e), ((int)e).ToString())));
            return await Task.FromResult(list);
        }

        private static string GetPropertyTypeAr(PropertyType t) => t switch
        {
            PropertyType.ResidentialBuilding => "سكني",
            PropertyType.CommercialBuilding => "تجاري",
            PropertyType.MixedUse => "مختلط",
            PropertyType.Agricultural => "زراعي",
            PropertyType.Mosque => "مسجد",
            PropertyType.School => "مدرسة",
            PropertyType.Hospital => "مستشفى",
            PropertyType.Land => "أرض",
            _ => t.ToString()
        };

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — GENERATE DEFAULT FLOORS
        // ─────────────────────────────────────────────────────────────────────
        private static List<FloorInputViewModel> GenerateDefaultFloors(int floorCount, int basementCount)
        {
            var floors = new List<FloorInputViewModel>();

            for (int b = basementCount; b >= 1; b--)
            {
                var floor = new FloorInputViewModel
                {
                    FloorNumber = -b,
                    FloorUsage = "Storage",
                    Units = new List<UnitInputViewModel>
                    {
                        new UnitInputViewModel { UnitNumber = $"B{b}-1", UnitType = "Warehouse", OccupancyStatus = "Vacant" }
                    }
                };
                floor.FloorLabel = floor.FloorDisplayName;
                floors.Add(floor);
            }

            for (int f = 0; f < floorCount; f++)
            {
                var floor = new FloorInputViewModel
                {
                    FloorNumber = f,
                    FloorUsage = f == 0 ? "Commercial" : "Residential",
                    Units = new List<UnitInputViewModel>
                    {
                        new UnitInputViewModel
                        {
                            UnitNumber = f == 0 ? "G-1" : $"{f}-1",
                            UnitType = f == 0 ? "Shop" : "Apartment",
                            OccupancyStatus = "Vacant"
                        }
                    }
                };
                floor.FloorLabel = floor.FloorDisplayName;
                floors.Add(floor);
            }

            return floors;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — GEO / GIS VALIDATION
        // ─────────────────────────────────────────────────────────────────────
        private static bool IsValidGeoJsonPolygon(string geoJson)
        {
            if (string.IsNullOrWhiteSpace(geoJson)) return false;
            try
            {
                using var doc = JsonDocument.Parse(geoJson);
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeEl)) return false;
                if (typeEl.GetString() != "Polygon") return false;
                if (!root.TryGetProperty("coordinates", out var coordsEl)) return false;
                var ring = coordsEl[0];
                return ring.GetArrayLength() >= 4;
            }
            catch { return false; }
        }

        private static bool IsWithinIraq(decimal lat, decimal lng)
            => lat >= 29m && lat <= 38m && lng >= 38m && lng <= 49m;

        private string GetGisBaseUrl() => _configuration["Gis:BaseUrl"] ?? string.Empty;

        private static string GetDefaultCenter(int governorateId) => governorateId switch
        {
            1 => "[33.3152, 44.3661]",
            2 => "[36.3361, 43.1189]",
            15 => "[30.5052, 47.7837]",
            _ => "[33.3152, 44.3661]"
        };

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — DQS COMPUTATION (client-side replica)
        // ─────────────────────────────────────────────────────────────────────
        private static List<DqsCriterionViewModel> ComputeDqsCriteria(PropertyCreateViewModel vm)
        {
            var criteria = new List<DqsCriterionViewModel>();

            bool nameOk = !string.IsNullOrWhiteSpace(vm.BasicInfo.NameAr) && vm.BasicInfo.NameAr!.Length >= 5;
            criteria.Add(new DqsCriterionViewModel { Label = "اسم العقار (≥5 أحرف)", Weight = 5, Achieved = nameOk });

            bool waqfTypeOk = !string.IsNullOrWhiteSpace(vm.BasicInfo.WaqfType);
            criteria.Add(new DqsCriterionViewModel { Label = "نوع الوقف", Weight = 5, Achieved = waqfTypeOk });

            bool propTypeOk = vm.BasicInfo.PropertyTypeId > 0;
            criteria.Add(new DqsCriterionViewModel { Label = "تصنيف العقار", Weight = 5, Achieved = propTypeOk });

            bool locationL4Ok = vm.Geographic.SubDistrictId > 0;
            criteria.Add(new DqsCriterionViewModel { Label = "الموقع حتى مستوى الناحية", Weight = 10, Achieved = locationL4Ok });

            bool locationL6Ok = vm.Geographic.StreetId.HasValue && vm.Geographic.StreetId.Value > 0;
            criteria.Add(new DqsCriterionViewModel { Label = "الموقع حتى مستوى الشارع", Weight = 5, Achieved = locationL6Ok });

            bool gpsOk = vm.MapLocation.Latitude.HasValue && vm.MapLocation.Longitude.HasValue;
            criteria.Add(new DqsCriterionViewModel { Label = "إحداثيات GPS", Weight = 15, Achieved = gpsOk });

            bool polyOk = !string.IsNullOrWhiteSpace(vm.MapLocation.GisPolygon);
            criteria.Add(new DqsCriterionViewModel { Label = "حدود المضلع (GeoJSON)", Weight = 10, Achieved = polyOk });

            bool deedOk = !string.IsNullOrWhiteSpace(vm.Documents.DeedNumber) && vm.Documents.DeedUploaded;
            criteria.Add(new DqsCriterionViewModel { Label = "رقم الصك + ملف مرفوع", Weight = 15, Achieved = deedOk });

            bool cadOk = !string.IsNullOrWhiteSpace(vm.Documents.CadastralNumber) || !string.IsNullOrWhiteSpace(vm.Documents.TabuNumber);
            criteria.Add(new DqsCriterionViewModel { Label = "رقم الكاداسترو أو الطابو", Weight = 5, Achieved = cadOk });

            bool condOk = !string.IsNullOrWhiteSpace(vm.BasicInfo.StructuralCondition);
            criteria.Add(new DqsCriterionViewModel { Label = "الحالة الإنشائية", Weight = 5, Achieved = condOk });

            bool photosOk = vm.Documents.UploadedPhotoCount >= 4;
            criteria.Add(new DqsCriterionViewModel { Label = "صور العقار (≥4 صور)", Weight = 10, Achieved = photosOk });

            bool valueOk = vm.BasicInfo.EstimatedValue.HasValue && vm.BasicInfo.EstimatedValue.Value > 0;
            criteria.Add(new DqsCriterionViewModel { Label = "القيمة التقديرية", Weight = 5, Achieved = valueOk });

            bool areaOk = vm.BasicInfo.TotalAreaSqm.HasValue && vm.BasicInfo.TotalAreaSqm.Value > 0;
            criteria.Add(new DqsCriterionViewModel { Label = "المساحة الكلية (م²)", Weight = 5, Achieved = areaOk });

            return criteria;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — MAP DRAFT TO DTO
        // ─────────────────────────────────────────────────────────────────────
        private static CreatePropertyDto MapDraftToCreateDto(PropertyCreateViewModel draft)
        {
            var propertyType = Enum.IsDefined(typeof(PropertyType), draft.BasicInfo.PropertyTypeId)
                ? (PropertyType)draft.BasicInfo.PropertyTypeId
                : PropertyType.CommercialBuilding;

            var propertyCategory = propertyType switch
            {
                PropertyType.Agricultural or PropertyType.Farm => PropertyCategory.Agricultural,
                PropertyType.Land => PropertyCategory.Land,
                _ => PropertyCategory.Building
            };

            var dto = new CreatePropertyDto
            {
                PropertyName = draft.BasicInfo.NameAr,
                PropertyType = propertyType,
                PropertyCategory = propertyCategory,
                DeedNumber = draft.Documents.DeedNumber,
                CadastralNumber = draft.Documents.CadastralNumber,
                TabuNumber = draft.Documents.TabuNumber,
                RegistrationDate = draft.Documents.DeedDate,
                FounderName = draft.BasicInfo.FounderName,
                FoundationDate = draft.BasicInfo.FoundingYear.HasValue
                    ? new DateTime(draft.BasicInfo.FoundingYear.Value, 1, 1)
                    : null,
                TotalFloors = (short?)draft.BuildingDetails.FloorCount,
                TotalAreaSqm = draft.BasicInfo.TotalAreaSqm,
                LandAreaSqm = propertyCategory != PropertyCategory.Building
                    ? draft.BasicInfo.TotalAreaSqm
                    : null,
                YearBuilt = (short?)draft.BuildingDetails.ConstructionYear,
                EstimatedValue = draft.BasicInfo.EstimatedValue,
                Latitude = draft.MapLocation.Latitude,
                Longitude = draft.MapLocation.Longitude,
                GpsAccuracyMeters = draft.MapLocation.GpsAccuracyMeters,
                GisPolygon = draft.MapLocation.GisPolygon,
                GovernorateId = draft.Geographic.GovernorateId > 0 ? draft.Geographic.GovernorateId : (int?)null,
                Notes = draft.BasicInfo.Notes,
                StreetId = draft.Geographic.StreetId,
                BuildingNumber = draft.Geographic.BuildingNumber,
                PlotNumber = draft.Geographic.PlotNumber,
                BlockNumber = draft.Geographic.BlockNumber,
                NearestLandmark = draft.Geographic.NearestLandmark
            };

            dto.OwnershipType = draft.BasicInfo.OwnershipType switch
            {
                "FULL_WAQF" => OwnershipType.FullWaqf,
                "PARTNERSHIP" => OwnershipType.Partnership,
                "AGRICULTURAL" => OwnershipType.Partnership,  // best-fit; no Agricultural variant
                _ => OwnershipType.FullWaqf
            };

            dto.OwnershipPercentage = draft.BasicInfo.OwnershipType == "PARTNERSHIP"
                && draft.BasicInfo.WaqfSharePercent.HasValue
                ? draft.BasicInfo.WaqfSharePercent.Value : 100m;

            if (!string.IsNullOrWhiteSpace(draft.BasicInfo.WaqfType)
                && Enum.TryParse<WaqfType>(draft.BasicInfo.WaqfType, out var wt))
                dto.WaqfType = wt;

            if (!string.IsNullOrWhiteSpace(draft.BasicInfo.StructuralCondition)
                && Enum.TryParse<StructuralCondition>(draft.BasicInfo.StructuralCondition, out var sc))
                dto.StructuralCondition = sc;

            return dto;
        }

        private static UpdatePropertyDto MapDraftToUpdateDto(PropertyCreateViewModel draft, int propertyId)
        {
            var create = MapDraftToCreateDto(draft);
            return new UpdatePropertyDto
            {
                Id = propertyId,
                PropertyName = create.PropertyName,
                PropertyType = create.PropertyType,
                PropertyCategory = create.PropertyCategory,
                WaqfType = create.WaqfType,
                OwnershipType = create.OwnershipType,
                OwnershipPercentage = create.OwnershipPercentage,
                DeedNumber = create.DeedNumber,
                CadastralNumber = create.CadastralNumber,
                TabuNumber = create.TabuNumber,
                RegistrationDate = create.RegistrationDate,
                WaqfOriginStory = create.WaqfOriginStory,
                FounderName = create.FounderName,
                FoundationDate = create.FoundationDate,
                EndowmentPurpose = create.EndowmentPurpose,
                TotalFloors = create.TotalFloors,
                TotalAreaSqm = create.TotalAreaSqm,
                LandAreaSqm = create.LandAreaSqm,
                YearBuilt = create.YearBuilt,
                ConstructionType = create.ConstructionType,
                StructuralCondition = create.StructuralCondition,
                EstimatedValue = create.EstimatedValue,
                Latitude = create.Latitude,
                Longitude = create.Longitude,
                GpsAccuracyMeters = create.GpsAccuracyMeters,
                GisPolygon = create.GisPolygon,
                GovernorateId = create.GovernorateId,
                Notes = create.Notes,
                StreetId = create.StreetId,
                BuildingNumber = create.BuildingNumber,
                PlotNumber = create.PlotNumber,
                BlockNumber = create.BlockNumber,
                NearestLandmark = create.NearestLandmark,
                LocalId = create.LocalId,
                DeviceId = create.DeviceId
            };
        }

        private static PropertyCreateViewModel MapDetailToDraft(PropertyDetailDto detail)
        {
            var draft = new PropertyCreateViewModel
            {
                BasicInfo =
                {
                    NameAr = detail.PropertyName,
                    OwnershipType = detail.OwnershipType == OwnershipType.Partnership ? "PARTNERSHIP" : "FULL_WAQF",
                    PropertyTypeId = (int)detail.PropertyType,
                    WaqfType = detail.WaqfType?.ToString(),
                    FounderName = detail.FounderName,
                    FoundingYear = detail.FoundationDate?.Year,
                    StructuralCondition = detail.StructuralCondition?.ToString(),
                    EstimatedValue = detail.EstimatedValue,
                    TotalAreaSqm = detail.TotalAreaSqm,
                    Notes = detail.Notes,
                    WaqfSharePercent = detail.OwnershipType == OwnershipType.Partnership ? detail.OwnershipPercentage : null
                },
                Geographic =
                {
                    GovernorateId = detail.Address?.GovernorateId ?? detail.GovernorateId ?? 0,
                    DistrictId = detail.Address?.DistrictId ?? 0,
                    SubDistrictId = detail.Address?.SubDistrictId ?? 0,
                    NeighborhoodId = detail.Address?.NeighborhoodId,
                    StreetId = detail.Address?.StreetId,
                    BuildingNumber = detail.Address?.BuildingNumber,
                    PlotNumber = detail.Address?.PlotNumber,
                    BlockNumber = detail.Address?.BlockNumber,
                    ZoneNumber = detail.Address?.ZoneNumber,
                    NearestLandmark = detail.Address?.NearestLandmark,
                    AlternativeAddress = detail.Address?.AlternativeAddress,
                    What3Words = detail.Address?.What3Words
                },
                MapLocation =
                {
                    Latitude = detail.Latitude,
                    Longitude = detail.Longitude,
                    GpsAccuracyMeters = detail.GpsAccuracyMeters,
                    GisPolygon = detail.GisPolygon
                },
                BuildingDetails =
                {
                    FloorCount = detail.TotalFloors ?? (detail.Floors?.Count ?? 1),
                    BasementCount = detail.BasementFloors ?? 0,
                    ConstructionYear = detail.YearBuilt,
                    PrimaryUsage = detail.PropertyType.ToString()
                },
                Documents =
                {
                    DeedNumber = detail.DeedNumber,
                    CadastralNumber = detail.CadastralNumber,
                    TabuNumber = detail.TabuNumber,
                    ExistingPhotoCount = detail.Photos?.Count ?? 0
                }
            };

            if (detail.AgriculturalDetail != null)
            {
                draft.BasicInfo.TotalAreaDunum = detail.AgriculturalDetail.TotalAreaDunum;
                draft.BasicInfo.CultivatedAreaDunum = detail.AgriculturalDetail.CultivatedAreaDunum;
                draft.BasicInfo.SoilType = detail.AgriculturalDetail.SoilType?.ToString();
                draft.BasicInfo.WaterSourceType = detail.AgriculturalDetail.WaterSourceType?.ToString();
                draft.BasicInfo.IrrigationMethod = detail.AgriculturalDetail.IrrigationMethod?.ToString();
                draft.BasicInfo.PrimaryHarvestType = detail.AgriculturalDetail.PrimaryHarvestType;
                draft.BasicInfo.SeasonType = detail.AgriculturalDetail.SeasonType?.ToString();
                draft.BasicInfo.FarmingContractType = detail.AgriculturalDetail.FarmingContractType?.ToString();
                draft.BasicInfo.WaqfShareOfHarvest = detail.AgriculturalDetail.WaqfShareOfHarvest;
                draft.BasicInfo.FarmerName = detail.AgriculturalDetail.FarmerName;
                draft.BasicInfo.FarmerNationalId = detail.AgriculturalDetail.FarmerNationalId;
            }

            var activePartnership = detail.Partnerships?.FirstOrDefault(p => p.IsActive) ?? detail.Partnerships?.FirstOrDefault();
            if (activePartnership != null)
            {
                draft.BasicInfo.PartnerName = activePartnership.PartnerName;
                draft.BasicInfo.PartnerType = activePartnership.PartnerType.ToString();
            }

            if (detail.Floors != null && detail.Floors.Any())
            {
                draft.BuildingDetails.Floors = detail.Floors
                    .OrderBy(f => f.FloorNumber)
                    .Select(f => new FloorInputViewModel
                    {
                        FloorNumber = f.FloorNumber,
                        FloorLabel = string.IsNullOrWhiteSpace(f.FloorLabel) ? $"الطابق {f.FloorNumber}" : f.FloorLabel,
                        FloorUsage = f.FloorUsage.ToString(),
                        AreaSqm = f.TotalAreaSqm,
                        StructuralCondition = f.StructuralCondition?.ToString(),
                        CeilingHeightCm = f.CeilingHeightCm,
                        HasBalcony = f.HasBalcony,
                        IsOccupied = f.IsOccupied,
                        Notes = f.Notes,
                        Units = f.Units.Select(u => new UnitInputViewModel
                        {
                            UnitNumber = u.UnitNumber ?? $"{f.FloorNumber}-{u.Id}",
                            UnitType = u.UnitType.ToString(),
                            AreaSqm = u.AreaSqm,
                            BedroomCount = u.BedroomCount ?? 0,
                            OccupancyStatus = u.OccupancyStatus.ToString(),
                            MarketRentMonthly = u.MarketRentMonthly,
                            ElectricMeterNo = u.ElectricMeterNo,
                            WaterMeterNo = u.WaterMeterNo
                        }).ToList()
                    }).ToList();
            }

            var deedDoc = detail.Documents.FirstOrDefault(d => d.DocumentCategory == DocumentCategory.Ownership);
            if (deedDoc != null)
            {
                draft.Documents.DeedDate = deedDoc.DocumentDate;
                draft.Documents.DeedCourt = deedDoc.IssuingAuthority;
                draft.Documents.DeedUploaded = true;
            }

            return draft;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — UPLOAD DOCUMENTS
        // ─────────────────────────────────────────────────────────────────────
        private async Task UploadDocumentsAsync(int propertyId, DocumentsViewModel docs)
        {
            var uploadErrors = new List<string>();

            if (docs.DeedFile != null)
            {
                var dto = new UploadDocumentDto
                {
                    PropertyId = propertyId,
                    DocumentCategory = DocumentCategory.Ownership,
                    DocumentType = "Deed",
                    DocumentNumber = docs.DeedNumber,
                    DocumentDate = docs.DeedDate,
                    IssuingAuthority = docs.DeedCourt
                };
                try { await _documentService.UploadAsync(docs.DeedFile, dto, CurrentUserId); }
                catch (Exception ex) { uploadErrors.Add($"صك الملكية: {ex.Message}"); }
            }

            if (docs.CadastralFile != null)
            {
                var dto = new UploadDocumentDto
                {
                    PropertyId = propertyId,
                    DocumentCategory = DocumentCategory.Survey,
                    DocumentType = "Cadastral",
                    DocumentNumber = docs.CadastralNumber ?? docs.TabuNumber
                };
                try { await _documentService.UploadAsync(docs.CadastralFile, dto, CurrentUserId); }
                catch (Exception ex) { uploadErrors.Add($"وثيقة الكاداسترو/الطابو: {ex.Message}"); }
            }

            if (docs.BuildingPermitFile != null)
            {
                var dto = new UploadDocumentDto
                {
                    PropertyId = propertyId,
                    DocumentCategory = DocumentCategory.Construction,
                    DocumentType = "BuildingPermit",
                    DocumentNumber = docs.BuildingPermitNumber,
                    DocumentDate = docs.BuildingPermitDate
                };
                try { await _documentService.UploadAsync(docs.BuildingPermitFile, dto, CurrentUserId); }
                catch (Exception ex) { uploadErrors.Add($"رخصة البناء: {ex.Message}"); }
            }

            if (docs.CompletionCertFile != null)
            {
                var dto = new UploadDocumentDto
                {
                    PropertyId = propertyId,
                    DocumentCategory = DocumentCategory.Construction,
                    DocumentType = "CompletionCert",
                    DocumentNumber = docs.CompletionCertNumber
                };
                try { await _documentService.UploadAsync(docs.CompletionCertFile, dto, CurrentUserId); }
                catch (Exception ex) { uploadErrors.Add($"شهادة الإنجاز: {ex.Message}"); }
            }

            var photoMap = new (IFormFile? File, PhotoType Type)[]
            {
                (docs.PhotoFront,  PhotoType.FrontFacade),
                (docs.PhotoRight,  PhotoType.Right),
                (docs.PhotoLeft,   PhotoType.Left),
                (docs.PhotoBack,   PhotoType.Back),
                (docs.PhotoInside, PhotoType.Interior)
            };

            foreach (var (file, photoType) in photoMap)
            {
                if (file != null)
                {
                    try { await _documentService.AddPhotoAsync(file, propertyId, photoType, CurrentUserId); }
                    catch (Exception ex) { uploadErrors.Add($"صورة {photoType}: {ex.Message}"); }
                }
            }

            if (uploadErrors.Any())
            {
                var combined = string.Join(" | ", uploadErrors);
                throw new InvalidOperationException($"فشل في رفع بعض الوثائق/الصور: {combined}");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SUPPORTING MODELS
    // ─────────────────────────────────────────────────────────────────────────
    public class SavePolygonRequest
    {
        public string DraftKey { get; set; } = string.Empty;
        public string GeoJson { get; set; } = string.Empty;
        public decimal? AreaSqm { get; set; }
    }

    public class PropertyIndexWrapperViewModel
    {
        public object? Results { get; set; }
        public PropertyFilterRequest Filter { get; set; } = new();
        public SelectList? Governorates { get; set; }
    }
}
