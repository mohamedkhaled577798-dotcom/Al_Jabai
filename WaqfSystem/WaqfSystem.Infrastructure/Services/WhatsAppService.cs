using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> SendAsync(string phone, string message)
        {
            try
            {
                var apiUrl = _configuration["WhatsApp:ApiUrl"];
                var token = _configuration["WhatsApp:Token"];
                var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];

                if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("WhatsApp configuration is missing");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var payload = new
                {
                    to = phone,
                    message,
                    phoneNumberId
                };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("WhatsApp send failed with status code {StatusCode}", response.StatusCode);
                    return null;
                }

                var body = await response.Content.ReadAsStringAsync();
                var msgId = !string.IsNullOrWhiteSpace(body) ? body : Guid.NewGuid().ToString("N");
                _logger.LogInformation("WhatsApp sent to {Phone}. MessageId: {MessageId}", phone, msgId);
                return msgId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp send failed for {Phone}", phone);
                return null;
            }
        }
    }
}
