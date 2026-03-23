using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Infrastructure.Data;
using WaqfSystem.Infrastructure.Repositories;
using WaqfSystem.Infrastructure.Services;

namespace WaqfSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=(localdb)\\mssqllocaldb;Database=WaqfSystem;Trusted_Connection=True;MultipleActiveResultSets=true";
            
            services.AddDbContext<WaqfDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<WaqfDbContext>());

            // Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();

            // External Services
            services.AddHttpClient<IGisSyncService, GisSyncService>();
            services.AddHttpClient<ISmsService, SmsService>();
            services.AddHttpClient<IWhatsAppService, WhatsAppService>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<WaqfSystem.Core.Interfaces.IOcrService, OcrService>();
            services.AddScoped<WaqfSystem.Application.Services.IOcrService, OcrService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IPartnershipExpiryJob, BackgroundJobs.PartnershipExpiryJob>();
            services.AddScoped<BackgroundJobs.SmartSuggestionRefresher>();
            services.AddScoped<BackgroundJobs.ScheduleOverdueChecker>();
            services.AddScoped<BackgroundJobs.ContractExpiryNotifier>();

            return services;
        }
    }
}
