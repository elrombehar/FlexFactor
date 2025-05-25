using DisputeReconsile.Interfaces;
using DisputeReconsile.Models;

namespace DisputeReconsile.Infra.Repos
{
    public class InMemDisputeRepo : IDisputeRepo
    {
        private readonly List<Dispute> _disputes;

        public InMemDisputeRepo()
        {
            _disputes = GenerateSampleDisputes();
        }

        public Task<IEnumerable<Dispute>> GetAllDisputesAsync()
            => Task.FromResult<IEnumerable<Dispute>>(_disputes);
        

        public Task<Dispute?> GetDisputeByIdAsync(string disputeId)
        {
            var dispute = _disputes.FirstOrDefault(d => d.DisputeId == disputeId);
            return Task.FromResult(dispute);
        }

        public Task<IEnumerable<Dispute>> GetDisputesByStatusAsync(string status)
        {
            var disputes = _disputes.Where(d =>
                string.Equals(d.Status, status, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(disputes);
        }

        public Task AddDisputeAsync(Dispute dispute)
        {
            _disputes.Add(dispute);
            return Task.CompletedTask;
        }

        public Task UpdateDisputeAsync(Dispute dispute)
        {
            var existingDispute = _disputes.FirstOrDefault(d => d.DisputeId == dispute.DisputeId);
            if (existingDispute != null)
            {
                var index = _disputes.IndexOf(existingDispute);
                _disputes[index] = dispute;
            }
            return Task.CompletedTask;
        }

        private static List<Dispute> GenerateSampleDisputes()
            => [
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Fraud"
                },
                new()
                {
                    DisputeId = "case_002",
                    TransactionId = "txn_005",
                    Amount = 150.00m,
                    Currency = "USD",
                    Status = "Lost",
                    Reason = "Product Not Received"
                },
                new()
                {
                    DisputeId = "case_004",
                    TransactionId = "txn_007",
                    Amount = 90.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Unauthorized"
                }
            ];
        
    }
}
