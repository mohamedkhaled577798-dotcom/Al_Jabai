using System.Threading.Tasks;

namespace WaqfSystem.Core.Interfaces
{
    public interface IGisSyncService
    {
        Task<bool> SyncPropertyToGisAsync(int propertyId);
        Task<bool> SyncPropertyFromGisAsync(int propertyId);
        Task ProcessPendingSyncsAsync();
    }
}
