namespace DisputeReconsile.Models
{
    public class DisputeCsvRecord
    {
        public string? DisputeId { get; set; }
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
    }
}
