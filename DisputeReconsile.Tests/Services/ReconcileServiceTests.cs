using DisputeReconsile.Interfaces;
using DisputeReconsile.Models;
using DisputeReconsile.Services;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace DisputeReconsile.Tests.Services
{
    public class ReconcileServiceTests
    {
        private readonly Mock<IExchangeRateService> _mockExchangeRateService;
        private readonly Mock<IAlertService> _mockAlertService;
        private readonly Mock<ILogger<ReconcileService>> _mockLogger;
        private readonly ReconcileService _reconciliationService;

        public ReconcileServiceTests()
        {
            _mockExchangeRateService = new Mock<IExchangeRateService>();
            _mockAlertService = new Mock<IAlertService>();
            _mockLogger = new Mock<ILogger<ReconcileService>>();

            _reconciliationService = new ReconcileService(
                _mockExchangeRateService.Object,
                _mockAlertService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ReconcileDisputesAsync_WithMatchingDisputes_ShouldReturnNoDiscrepancies()
        {
            // Arrange
            var externalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Fraud"
                }
            };

            var internalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Fraud"
                }
        };

            // Act
            var result = await _reconciliationService.ReconcileDisputesAsync(externalDisputes, internalDisputes);

            // Assert
            result.Should().NotBeNull();
            result.Discrepancies.Should().BeEmpty();
            result.Summary.TotalDiscrepancies.Should().Be(0);
            result.Summary.TotalExternalRecords.Should().Be(1);
            result.Summary.TotalInternalRecords.Should().Be(1);
        }

        [Fact]
        public async Task ReconcileDisputesAsync_WithMissingInternalDispute_ShouldDetectMissingDiscrepancy()
        {
            // Arrange
            var externalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Fraud"
                }
            };

            var internalDisputes = new List<Dispute>();

            // Act
            var result = await _reconciliationService.ReconcileDisputesAsync(externalDisputes, internalDisputes);

            // Assert
            result.Should().NotBeNull();
            result.Discrepancies.Should().HaveCount(1);
            result.Discrepancies[0].Type.Should().Be(DiscrepancyType.MissingInInternal);
            result.Discrepancies[0].DisputeId.Should().Be("case_001");
            result.Summary.MissingInInternal.Should().Be(1);
        }

        [Fact]
        public async Task ReconcileDisputesAsync_WithStatusMismatch_ShouldDetectStatusDiscrepancy()
        {
            // Arrange
            var externalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Won",
                    Reason = "Fraud"
                }
            };

            var internalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Lost",
                    Reason = "Fraud"
                }
            };

            // Act
            var result = await _reconciliationService.ReconcileDisputesAsync(externalDisputes, internalDisputes);

            // Assert
            result.Should().NotBeNull();
            result.Discrepancies.Should().HaveCount(1);
            result.Discrepancies[0].Type.Should().Be(DiscrepancyType.StatusMismatch);
            result.Discrepancies[0].Severity.Should().Be(SeverityLevel.High);
            result.Summary.StatusMismatches.Should().Be(1);
        }

        [Fact]
        public async Task ReconcileDisputesAsync_WithAmountMismatch_ShouldDetectAmountDiscrepancy()
        {
            // Arrange
            var externalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Fraud"
                }
            };

            var internalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 150.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Fraud"
                }
            };

            // Act
            var result = await _reconciliationService.ReconcileDisputesAsync(externalDisputes, internalDisputes);

            // Assert
            result.Should().NotBeNull();
            result.Discrepancies.Should().HaveCount(1);
            result.Discrepancies[0].Type.Should().Be(DiscrepancyType.AmountMismatch);
            result.Discrepancies[0].Severity.Should().Be(SeverityLevel.Medium);
            result.Summary.AmountMismatches.Should().Be(1);
        }

        [Fact]
        public async Task ReconcileDisputesAsync_WithCurrencyMismatch_ShouldHandleCurrencyConversion()
        {
            // Arrange
            var externalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Fraud"
                }
            };

            var internalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 85.00m,
                    Currency = "EUR",
                    Status = "Open",
                    Reason = "Fraud"
                }
            };

            _mockExchangeRateService
                .Setup(x => x.ConvertAmountAsync(100.00m, "USD", "EUR"))
                .ReturnsAsync(85.00m);

            // Act
            var result = await _reconciliationService.ReconcileDisputesAsync(externalDisputes, internalDisputes);

            // Assert
            result.Should().NotBeNull();
            result.Discrepancies.Should().HaveCount(1); // Only currency mismatch, amounts match after conversion
            result.Discrepancies[0].Type.Should().Be(DiscrepancyType.CurrencyMismatch);
        }

        [Fact]
        public async Task ReconcileDisputesAsync_WithHighSeverityDiscrepancies_ShouldSendAlert()
        {
            // Arrange
            var externalDisputes = new List<Dispute>
            {
                new()
                {
                    DisputeId = "case_001",
                    TransactionId = "txn_001",
                    Amount = 1000.00m,
                    Currency = "USD",
                    Status = "Open",
                    Reason = "Fraud"
                }
            };

            var internalDisputes = new List<Dispute>();

            // Act
            await _reconciliationService.ReconcileDisputesAsync(externalDisputes, internalDisputes);

            // Assert
            _mockAlertService.Verify(
                x => x.SendHighSeverityDiscrepancyAlertAsync(It.IsAny<IEnumerable<DisputeDiscrepancy>>()),
                Times.Once);
        }
    }
}