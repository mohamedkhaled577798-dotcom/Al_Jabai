using System.Threading.Tasks;

namespace WaqfSystem.Core.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string to, string subject, string htmlBody);
        Task<bool> SendWithAttachmentAsync(string to, string subject, string htmlBody, byte[] attachment, string attachmentName);
    }
}
