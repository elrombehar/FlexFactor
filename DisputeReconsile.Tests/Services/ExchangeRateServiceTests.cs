using DisputeReconsile.Infra.ExchangeRate;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DisputeReconsile.Tests.Services
{
    public class ExchangeRateServiceTests
    {
        private readonly Mock<ILogger<ExchangeRateService>> _mockLogger;
        private readonly ExchangeRateService _exchangeRateService;

        public ExchangeRateServiceTests()
        {
            _mockLogger = new Mock<ILogger<ExchangeRateService>>();
            _exchangeRateService = new ExchangeRateService(_mockLogger.Object);
        }

        [Fact]
        public async Task ConvertAmountAsync_WithSameCurrency_ShouldReturnSameAmount()
        {
            // Arrange
            const decimal amount = 100.00m;
            const string currency = "USD";

            // Act
            var result = await _exchangeRateService.ConvertAmountAsync(amount, currency, currency);

            // Assert
            result.Should().Be(amount);
        }

        [Fact]
        public async Task ConvertAmountAsync_WithValidCurrencyPair_ShouldReturnConvertedAmount()
        {
            // Arrange
            const decimal amount = 100.00m;
            const string fromCurrency = "USD";
            const string toCurrency = "EUR";

            // Act
            var result = await _exchangeRateService.ConvertAmountAsync(amount, fromCurrency, toCurrency);

            // Assert
            result.Should().Be(85.00m); 
        }

        [Fact]
        public async Task ConvertAmountAsync_WithInverseCurrencyPair_ShouldReturnConvertedAmount()
        {
            // Arrange
            const decimal amount = 85.00m;
            const string fromCurrency = "EUR";
            const string toCurrency = "USD";

            // Act
            var result = await _exchangeRateService.ConvertAmountAsync(amount, fromCurrency, toCurrency);

            // Assert
            result.Should().BeApproximately(100.30m, 0.01m); 
        }

        [Fact]
        public async Task ConvertAmountAsync_WithUnsupportedCurrency_ShouldThrowException()
        {
            // Arrange
            const decimal amount = 100.00m;
            const string fromCurrency = "JPY";
            const string toCurrency = "USD";

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                _exchangeRateService.ConvertAmountAsync(amount, fromCurrency, toCurrency));
        }

        [Fact]
        public async Task GetExchangeRateAsync_WithValidCurrencyPair_ShouldReturnRate()
        {
            // Arrange
            const string fromCurrency = "USD";
            const string toCurrency = "EUR";

            // Act
            var rate = await _exchangeRateService.GetExchangeRateAsync(fromCurrency, toCurrency);

            // Assert
            rate.Should().Be(0.85m);
        }
    }
}
