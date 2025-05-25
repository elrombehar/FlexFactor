using DisputeReconsile.Models;

namespace DisputeReconsile.Interfaces
{
    public interface IReconcileService
    {
        Task<ReconcileResult> ReconcileDisputesAsync(IEnumerable<Dispute> externalDisputes, IEnumerable<Dispute> internalDisputes);
    }
}
