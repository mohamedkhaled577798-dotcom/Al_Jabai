using System.Threading.Tasks;

namespace WaqfSystem.Application.Services
{
    public interface IPermissionDiscoveryService
    {
        Task SyncPermissionsAsync();
    }
}
