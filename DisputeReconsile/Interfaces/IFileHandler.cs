using DisputeReconsile.Models;

namespace DisputeReconsile.Interfaces
{
    public interface IFileHandler
    {
        bool CanHandle(string filePath);
        Task<IEnumerable<Dispute>> ReadDisputesAsync(string filePath);
        Task WriteReconcileResultAsync(ReconcileResult result, string outputPath);
    }
}
