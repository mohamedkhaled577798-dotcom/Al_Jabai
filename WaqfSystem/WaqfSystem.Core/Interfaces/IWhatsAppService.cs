using System.Threading.Tasks;

namespace WaqfSystem.Core.Interfaces
{
    public interface IWhatsAppService
    {
        Task<string?> SendAsync(string phone, string message);
    }
}
