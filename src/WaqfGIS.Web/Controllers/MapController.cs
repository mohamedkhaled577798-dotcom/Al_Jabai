using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class MapController : Controller
{
    private readonly MosqueService _mosqueService;
    private readonly PropertyService _propertyService;

    public MapController(MosqueService mosqueService, PropertyService propertyService)
    {
        _mosqueService = mosqueService;
        _propertyService = propertyService;
    }

    public IActionResult Index()
    {
        return View();
    }
}
