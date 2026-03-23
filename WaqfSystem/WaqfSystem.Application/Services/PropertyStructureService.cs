using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Application.DTOs.Revenue;

namespace WaqfSystem.Application.Services
{
    public class PropertyStructureService : IPropertyStructureService
    {
        private readonly IAppDbContext _db;
        private readonly IRevenueCollectionService _revenueCollectionService;

        public PropertyStructureService(IAppDbContext db, IRevenueCollectionService revenueCollectionService)
        {
            _db = db;
            _revenueCollectionService = revenueCollectionService;
        }

        public async Task<PropertyStructureDto> GetStructureAsync(long propertyId, string periodLabel, int userId)
        {
            var pId = (int)propertyId;
            var property = await _db.Properties.AsNoTracking().FirstAsync(x => x.Id == pId);
            var floors = await _db.PropertyFloors.AsNoTracking().Where(x => !x.IsDeleted && x.PropertyId == pId).OrderBy(x => x.FloorNumber).ToListAsync();
            var units = await _db.PropertyUnits.AsNoTracking().Where(x => !x.IsDeleted && x.PropertyId == pId).OrderBy(x => x.UnitNumber).ToListAsync();

            var dto = new PropertyStructureDto
            {
                PropertyId = propertyId,
                PropertyNameAr = property.PropertyName ?? "بدون اسم"
            };

            foreach (var floor in floors)
            {
                var floorCollision = await _revenueCollectionService.CheckCollisionAsync(propertyId, "Floor", floor.Id, null, periodLabel);
                var floorDto = new FloorStructureDto
                {
                    FloorId = floor.Id,
                    FloorLabel = floor.FloorLabel ?? $"طابق {floor.FloorNumber}",
                    IsLocked = floorCollision.HasCollision,
                    LockReason = floorCollision.HasCollision ? floorCollision.Message : null
                };

                foreach (var unit in units.Where(x => x.FloorId == floor.Id))
                {
                    var unitCollision = await _revenueCollectionService.CheckCollisionAsync(propertyId, "Unit", floor.Id, unit.Id, periodLabel);
                    floorDto.Units.Add(new UnitStructureDto
                    {
                        UnitId = unit.Id,
                        UnitNumber = unit.UnitNumber ?? "-",
                        UnitType = unit.UnitType.ToString(),
                        IsLocked = unitCollision.HasCollision,
                        LockReason = unitCollision.HasCollision ? unitCollision.Message : null
                    });
                }

                dto.Floors.Add(floorDto);
            }

            return dto;
        }
    }
}
