using DisputeReconsile.Models;

namespace DisputeReconsile.Interfaces
{
    public interface IDisputeRepo
    {
        Task<IEnumerable<Dispute>> GetAllDisputesAsync();
        Task<Dispute?> GetDisputeByIdAsync(string disputeId);
        Task<IEnumerable<Dispute>> GetDisputesByStatusAsync(string status);
        Task AddDisputeAsync(Dispute dispute);
        Task UpdateDisputeAsync(Dispute dispute);
    }
}
