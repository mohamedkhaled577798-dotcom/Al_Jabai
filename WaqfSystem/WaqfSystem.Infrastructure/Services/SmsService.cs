using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(HttpClient httpClient, IConfiguration configuration, ILogger<SmsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> SendAsync(string phone, string message)
        {
            try
            {
                var apiUrl = _configuration["Sms:ApiUrl"];
                var apiKey = _configuration["Sms:ApiKey"];
                var senderId = _configuration["Sms:SenderId"] ?? "WAQF";

                if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("SMS configuration is missing");
                    return null;
                }

                var payload = new
                {
                    to = phone,
                    text = message,
                    sender = senderId,
                    apiKey
                };

                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SMS send failed with status code {StatusCode}", response.StatusCode);
                    return null;
                }

                var body = await response.Content.ReadAsStringAsync();
                var reference = !string.IsNullOrWhiteSpace(body) ? body : Guid.NewGuid().ToString("N");
                _logger.LogInformation("SMS sent to {Phone}. Reference: {Reference}", phone, reference);
                return reference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS send failed for {Phone}", phone);
                return null;
            }
        }
    }
}
