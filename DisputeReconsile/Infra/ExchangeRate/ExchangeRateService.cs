using DisputeReconsile.Interfaces;
using Microsoft.Extensions.Logging;

namespace DisputeReconsile.Infra.ExchangeRate
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly ILogger<ExchangeRateService> _logger;
        private readonly Dictionary<string, decimal> _exchangeRates;

        public ExchangeRateService(ILogger<ExchangeRateService> logger)
        {
            _logger = logger;
            _exchangeRates = new Dictionary<string, decimal>
            {
                ["USD_EUR"] = 0.85m,
                ["EUR_USD"] = 1.18m,
                ["USD_GBP"] = 0.73m,
                ["GBP_USD"] = 1.37m,
                ["EUR_GBP"] = 0.86m,
                ["GBP_EUR"] = 1.16m
            };
        }

        public Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(amount);
            }

            var rateKey = $"{fromCurrency.ToUpper()}_{toCurrency.ToUpper()}";

            if (_exchangeRates.TryGetValue(rateKey, out var rate))
            {
                var convertedAmount = amount * rate;
                _logger.LogDebug("Converted {Amount} {FromCurrency} to {ConvertedAmount} {ToCurrency} using rate {Rate}",
                    amount, fromCurrency, convertedAmount, toCurrency, rate);
                return Task.FromResult(convertedAmount);
            }

            // If direct rate not found, try inverse
            var inverseKey = $"{toCurrency.ToUpper()}_{fromCurrency.ToUpper()}";
            if (_exchangeRates.TryGetValue(inverseKey, out var inverseRate))
            {
                var convertedAmount = amount / inverseRate;
                _logger.LogDebug("Converted {Amount} {FromCurrency} to {ConvertedAmount} {ToCurrency} using inverse rate {Rate}",
                    amount, fromCurrency, convertedAmount, toCurrency, 1 / inverseRate);
                return Task.FromResult(convertedAmount);
            }

            _logger.LogWarning("Exchange rate not found for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            throw new NotSupportedException($"Exchange rate not available for {fromCurrency} to {toCurrency}");
        }

        public Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(1.0m);
            }

            var rateKey = $"{fromCurrency.ToUpper()}_{toCurrency.ToUpper()}";

            if (_exchangeRates.TryGetValue(rateKey, out var rate))
            {
                return Task.FromResult(rate);
            }

            var inverseKey = $"{toCurrency.ToUpper()}_{fromCurrency.ToUpper()}";
            if (_exchangeRates.TryGetValue(inverseKey, out var inverseRate))
            {
                return Task.FromResult(1 / inverseRate);
            }

            throw new NotSupportedException($"Exchange rate not available for {fromCurrency} to {toCurrency}");
        }
    }
}
