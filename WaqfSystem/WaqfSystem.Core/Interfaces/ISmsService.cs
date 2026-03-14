using System.Threading.Tasks;

namespace WaqfSystem.Core.Interfaces
{
    public interface ISmsService
    {
        Task<string?> SendAsync(string phone, string message);
    }
}
