using System.Threading.Tasks;

namespace WaqfSystem.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendToRoleAsync(string roleCode, string title, string message, string? referenceTable = null, int? referenceId = null);
    }
}
