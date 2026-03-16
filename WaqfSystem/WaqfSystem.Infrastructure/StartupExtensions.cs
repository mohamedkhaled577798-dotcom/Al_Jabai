using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.Services;
using WaqfSystem.Infrastructure.Authorization;

namespace WaqfSystem.Infrastructure
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddAdminPanel(this IServiceCollection services, IConfiguration config)
        {
            services.AddMemoryCache();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IPermissionCacheService, PermissionCacheService>();
            services.AddScoped<IPermissionDiscoveryService, PermissionDiscoveryService>();
            services.AddScoped<IGeographicService, GeographicService>();
            return services;
        }

        public static IApplicationBuilder UseAdminPanel(this IApplicationBuilder app)
        {
            app.UseMiddleware<DynamicAuthorizationMiddleware>();

            using var scope = app.ApplicationServices.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminPanelStartup");
            var discovery = scope.ServiceProvider.GetRequiredService<IPermissionDiscoveryService>();
            discovery.SyncPermissionsAsync().GetAwaiter().GetResult();
            logger.LogInformation("Admin panel permission discovery synced successfully");

            return app;
        }
    }
}
