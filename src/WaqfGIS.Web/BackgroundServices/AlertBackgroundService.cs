using WaqfGIS.Services;

namespace WaqfGIS.Web.BackgroundServices;

/// <summary>
/// خدمة خلفية تُشغَّل كل ساعة لتوليد التنبيهات التلقائية
/// </summary>
public class AlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AlertBackgroundService> _logger;

    public AlertBackgroundService(IServiceProvider services, ILogger<AlertBackgroundService> logger)
    {
        _services = services;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertBackgroundService started.");
        await RunAlertsAsync(); // تشغيل فوري

        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RunAlertsAsync();
    }

    private async Task RunAlertsAsync()
    {
        try
        {
            using var scope  = _services.CreateScope();
            var alertService = scope.ServiceProvider.GetRequiredService<AlertService>();
            await alertService.GenerateAllAlertsAsync();
            _logger.LogInformation("Alerts generated at {Time}", DateTime.Now);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error generating alerts"); }
    }
}
