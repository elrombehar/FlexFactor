using System.ComponentModel.DataAnnotations;

namespace DisputeReconsile.Models
{
    public class Dispute
    {
        [Required]
        public string DisputeId { get; set; } = string.Empty;

        [Required]
        public string TransactionId { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

    }
}
