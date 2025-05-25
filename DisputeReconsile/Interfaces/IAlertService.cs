using DisputeReconsile.Models;

namespace DisputeReconsile.Interfaces
{
    public interface IAlertService
    {
        Task SendAlertAsync(string message, SeverityLevel severity);
        Task SendHighSeverityDiscrepancyAlertAsync(IEnumerable<DisputeDiscrepancy> discrepancies);
    }
}
