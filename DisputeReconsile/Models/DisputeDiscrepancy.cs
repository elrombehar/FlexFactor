namespace DisputeReconsile.Models
{
    public class DisputeDiscrepancy
    {
        public string DisputeId { get; set; } = string.Empty;
        public DiscrepancyType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dispute? ExternalDispute { get; set; }
        public Dispute? InternalDispute { get; set; }
        public SeverityLevel Severity { get; set; }
    }
}
