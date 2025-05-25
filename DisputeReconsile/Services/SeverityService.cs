using DisputeReconsile.Models;

namespace DisputeReconsile.Services
{
    public static class SeverityService
    {
        public static SeverityLevel DetermineSeverity(Dispute? externalDispute, Dispute? internalDispute, DiscrepancyType type)
            => type switch
            {
                DiscrepancyType.MissingInInternal => DetermineMissingInternalSeverity(externalDispute),
                DiscrepancyType.MissingInExternal => DetermineMissingExternalSeverity(internalDispute),
                _ => SeverityLevel.Medium
            };

        public static SeverityLevel DetermineMissingInternalSeverity(Dispute? externalDispute)
        {
            if (externalDispute == null) return SeverityLevel.Medium;

            // High severity if dispute is still open and has significant amount
            if (string.Equals(externalDispute.Status, "Open", StringComparison.OrdinalIgnoreCase) &&
                externalDispute.Amount > 500)  // Can be set as extranal config
            {
                return SeverityLevel.High;
            }

            return externalDispute.Amount > 100 ? SeverityLevel.Medium : SeverityLevel.Low;
        }

        public static SeverityLevel DetermineMissingExternalSeverity(Dispute? internalDispute)
        {
            if (internalDispute == null) return SeverityLevel.Medium;

            // Low severity if dispute is already resolved internally
            if (string.Equals(internalDispute.Status, "Won", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(internalDispute.Status, "Lost", StringComparison.OrdinalIgnoreCase))
            {
                return SeverityLevel.Low;
            }

            return SeverityLevel.Medium;
        }

        public static SeverityLevel DetermineStatusMismatchSeverity(string externalStatus, string internalStatus)
        {
            // High severity if external shows won but internal shows lost or vice versa
            // Can be set as external config
            var criticalMismatches = new[]
            {
                ("Won", "Lost"), ("Lost", "Won"),
                ("Won", "Open"), ("Lost", "Open")
            };

            if (criticalMismatches.Any(m =>
                string.Equals(externalStatus, m.Item1, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(internalStatus, m.Item2, StringComparison.OrdinalIgnoreCase)))
            {
                return SeverityLevel.High;
            }

            return SeverityLevel.Medium;
        }

        public static SeverityLevel DetermineAmountMismatchSeverity(decimal difference)
            // Can be set as external config
            => difference switch
            {
                >= 1000 => SeverityLevel.Critical,
                >= 100 => SeverityLevel.High,
                >= 10 => SeverityLevel.Medium,
                _ => SeverityLevel.Low
            };
    }
}
