using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public interface IGeographicService
    {
        Task<List<Governorate>> GetGovernoratesAsync();
    }

    public class GeographicService : IGeographicService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GeographicService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Governorate>> GetGovernoratesAsync()
        {
            return await _unitOfWork.GetQueryable<Governorate>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.NameAr)
                .ToListAsync();
        }
    }
}
