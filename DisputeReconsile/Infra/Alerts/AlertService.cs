using DisputeReconsile.Interfaces;
using DisputeReconsile.Models;
using Microsoft.Extensions.Logging;

namespace DisputeReconsile.Infra.Alerts
{
    public class AlertService(ILogger<AlertService> logger) : IAlertService
    {
        private readonly ILogger<AlertService> _logger = logger;

        public Task SendAlertAsync(string message, SeverityLevel severity)
        {
            var alertMessage = $"[{severity.ToString().ToUpper()}] {message}";

            Console.WriteLine(alertMessage);
            _logger.LogWarning("Alert sent: {alertMessage}", alertMessage);

            return Task.CompletedTask;
        }

        public async Task SendHighSeverityDiscrepancyAlertAsync(IEnumerable<DisputeDiscrepancy> discrepancies)
        {
            var discrepancyList = discrepancies.ToList();
            var criticalCount = discrepancyList.Count(d => d.Severity == SeverityLevel.Critical);
            var highCount = discrepancyList.Count(d => d.Severity == SeverityLevel.High);

            var alertMessage = $"High severity discrepancies detected: {criticalCount} Critical, {highCount} High priority issues found during reconcile process";

            await SendAlertAsync(alertMessage, SeverityLevel.High);

            foreach (var critical in discrepancyList.Where(d => d.Severity == SeverityLevel.Critical).Take(3))
            {
                 Console.WriteLine($"CRITICAL: {critical.DisputeId} - {critical.Description}");
            }

            _logger.LogError("High severity discrepancies alert: {Message}", alertMessage);
        }
    }
}
