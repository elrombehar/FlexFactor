namespace DisputeReconsile.Models
{
    public class ReconcileResult
    {
        public List<DisputeDiscrepancy> Discrepancies { get; set; } = [];
        public ReconcileSummary Summary { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public static class ReconcileResultExtensions
    {
        public static void DisplaySummary(this ReconcileResult result)
        {
            Console.WriteLine("\nRECONCILE SUMMARY");
            Console.WriteLine("=" + new string('=', 50));
            Console.WriteLine($"Total External Records: {result.Summary.TotalExternalRecords}");
            Console.WriteLine($"Total Internal Records: {result.Summary.TotalInternalRecords}");
            Console.WriteLine($"Total Discrepancies: {result.Summary.TotalDiscrepancies}");

            if (result.Summary.TotalDiscrepancies > 0)
            {
                Console.WriteLine("\nDISCREPANCY BREAKDOWN:");
                Console.WriteLine($"Missing in Internal: {result.Summary.MissingInInternal}");
                Console.WriteLine($"Missing in External: {result.Summary.MissingInExternal}");
                Console.WriteLine($"Status Mismatches: {result.Summary.StatusMismatches}");
                Console.WriteLine($"Amount Mismatches: {result.Summary.AmountMismatches}");

                if (result.Summary.HighSeverityDiscrepancies > 0)
                {
                    Console.WriteLine($"\nHIGH SEVERITY DISCREPANCIES: {result.Summary.HighSeverityDiscrepancies}");

                    var highSeverityItems = result.Discrepancies
                        .Where(d => d.Severity >= SeverityLevel.High)
                        .Take(5)
                        .ToList();

                    foreach (var discrepancy in highSeverityItems)
                    {
                        Console.WriteLine($" {discrepancy.DisputeId}: {discrepancy.Description}");
                    }

                    if (result.Summary.HighSeverityDiscrepancies > 5)
                    {
                        Console.WriteLine($" ... and {result.Summary.HighSeverityDiscrepancies - 5} more");
                    }
                }
            }
            else
            {
                Console.WriteLine("No discrepancies found - all records match!");
            }

            Console.WriteLine($"\nProcessed at: {result.ProcessedAt:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine("=" + new string('=', 50));
        }
    }
}
