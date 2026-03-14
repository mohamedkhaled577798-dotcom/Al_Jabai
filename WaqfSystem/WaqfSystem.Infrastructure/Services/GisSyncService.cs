using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Infrastructure.Data;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.Services
{

    public class GisSyncService : IGisSyncService
    {
        private readonly WaqfDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GisSyncService> _logger;

        public GisSyncService(
            WaqfDbContext context,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GisSyncService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SyncPropertyToGisAsync(int propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found for GIS sync", (object)propertyId);
                return false;
            }

            var syncLog = new GisSyncLog
            {
                PropertyId = propertyId,
                Direction = GisSyncDirection.ToGis,
                Status = GisSyncStatus.Pending,
                AttemptedAt = DateTime.UtcNow
            };

            try
            {
                var gisEndpoint = _configuration["GisSettings:ApiUrl"] ?? "https://gis.waqf.gov.iq/api";
                var payload = new
                {
                    featureId = property.GisFeatureId,
                    wqfNumber = property.WqfNumber,
                    name = property.PropertyName,
                    latitude = property.Latitude,
                    longitude = property.Longitude,
                    polygon = property.GisPolygon,
                    propertyType = property.PropertyType.ToString(),
                    layerName = property.GisLayerName ?? "waqf_properties"
                };

                syncLog.RequestPayload = JsonSerializer.Serialize(payload);

                var content = new StringContent(
                    syncLog.RequestPayload,
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{gisEndpoint}/features/sync", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                syncLog.ResponsePayload = responseBody;

                if (response.IsSuccessStatusCode)
                {
                    syncLog.Status = GisSyncStatus.Synced;
                    syncLog.CompletedAt = DateTime.UtcNow;

                    property.GisSyncStatus = GisSyncStatus.Synced;
                    property.LastGisSyncAt = DateTime.UtcNow;
                    property.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation("GIS sync completed for property {PropertyId}", propertyId);
                }
                else
                {
                    syncLog.Status = GisSyncStatus.Failed;
                    syncLog.ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody}";
                    property.GisSyncStatus = GisSyncStatus.Failed;

                    _logger.LogWarning("GIS sync failed for property {PropertyId}: {StatusCode}", (object)propertyId, (object)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                syncLog.Status = GisSyncStatus.Failed;
                syncLog.ErrorMessage = ex.Message;
                property.GisSyncStatus = GisSyncStatus.Failed;

                _logger.LogError(ex, "GIS sync error for property {PropertyId}", propertyId);
            }

            await _context.GisSyncLogs.AddAsync(syncLog);
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();

            return syncLog.Status == GisSyncStatus.Synced;
        }

        public async Task<bool> SyncPropertyFromGisAsync(int propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null || string.IsNullOrEmpty(property.GisFeatureId))
            {
                _logger.LogWarning("Property {PropertyId} not found or missing GIS feature ID", propertyId);
                return false;
            }

            var syncLog = new GisSyncLog
            {
                PropertyId = propertyId,
                Direction = GisSyncDirection.FromGis,
                Status = GisSyncStatus.Pending,
                AttemptedAt = DateTime.UtcNow
            };

            try
            {
                var gisEndpoint = _configuration["GisSettings:ApiUrl"] ?? "https://gis.waqf.gov.iq/api";

                var response = await _httpClient.GetAsync($"{gisEndpoint}/features/{property.GisFeatureId}");
                var responseBody = await response.Content.ReadAsStringAsync();
                syncLog.ResponsePayload = responseBody;

                if (response.IsSuccessStatusCode)
                {
                    var gisData = JsonSerializer.Deserialize<JsonElement>(responseBody);

                    if (gisData.TryGetProperty("polygon", out var polygon))
                        property.GisPolygon = polygon.GetString();
                    if (gisData.TryGetProperty("latitude", out var lat))
                        property.Latitude = lat.GetDecimal();
                    if (gisData.TryGetProperty("longitude", out var lng))
                        property.Longitude = lng.GetDecimal();

                    syncLog.Status = GisSyncStatus.Synced;
                    syncLog.CompletedAt = DateTime.UtcNow;
                    property.GisSyncStatus = GisSyncStatus.Synced;
                    property.LastGisSyncAt = DateTime.UtcNow;
                    property.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation("GIS pull sync completed for property {PropertyId}", propertyId);
                }
                else
                {
                    syncLog.Status = GisSyncStatus.Failed;
                    syncLog.ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody}";
                    _logger.LogWarning("GIS pull sync failed for property {PropertyId}", (object)propertyId);
                }
            }
            catch (Exception ex)
            {
                syncLog.Status = GisSyncStatus.Failed;
                syncLog.ErrorMessage = ex.Message;
                _logger.LogError(ex, "GIS pull sync error for property {PropertyId}", propertyId);
            }

            await _context.GisSyncLogs.AddAsync(syncLog);
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();

            return syncLog.Status == GisSyncStatus.Synced;
        }

        public async Task ProcessPendingSyncsAsync()
        {
            var pendingProperties = await _context.Properties
                .Where(p => p.GisSyncStatus == GisSyncStatus.Pending && p.Latitude.HasValue && p.Longitude.HasValue)
                .Select(p => p.Id)
                .ToListAsync();

            _logger.LogInformation("Processing {Count} pending GIS syncs", pendingProperties.Count);

            foreach (var propertyId in pendingProperties)
            {
                await SyncPropertyToGisAsync(propertyId);
            }
        }
    }
}
