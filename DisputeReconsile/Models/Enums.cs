namespace DisputeReconsile.Models
{
    public enum DiscrepancyType
    {
        MissingInInternal,
        MissingInExternal,
        StatusMismatch,
        AmountMismatch,
        CurrencyMismatch,
        ReasonMismatch
    }

    public enum SeverityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
}
