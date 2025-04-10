using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Services
{
    public class VisitNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VisitNotificationService> _logger;

        public VisitNotificationService(IServiceProvider serviceProvider, ILogger<VisitNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTime.Now;
                    var upcomingVisits = context.Visits
                        .Where(v => !v.IsValidated && v.ScheduledDate <= now.AddHours(24) && v.ScheduledDate > now)
                        .ToList();

                    foreach (var visit in upcomingVisits)
                    {
                        _logger.LogInformation($"Notifying commercial {visit.CommercialId} about upcoming visit {visit.Id} on {visit.ScheduledDate}");
                        // TODO: Implement actual notification (e.g., email, push notification)
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Check every hour
            }
        }
    }
}