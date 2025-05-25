namespace DisputeReconsile.Models
{
    public class ReconcileSummary
    {
        public int TotalExternalRecords { get; set; }
        public int TotalInternalRecords { get; set; }
        public int TotalDiscrepancies { get; set; }
        public int MissingInInternal { get; set; }
        public int MissingInExternal { get; set; }
        public int StatusMismatches { get; set; }
        public int AmountMismatches { get; set; }
        public int HighSeverityDiscrepancies { get; set; }
    }
}
