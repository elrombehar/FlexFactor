namespace DisputeReconsile.Interfaces
{
    public interface IExchangeRateService
    {
        Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency);
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    }
}
