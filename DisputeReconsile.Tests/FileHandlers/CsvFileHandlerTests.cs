using DisputeReconsile.Models;
using DisputeReconsile.Infra.FileHandlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DisputeReconsile.Tests.FileHandlers
{
    public class CsvFileHandlerTests
    {
        private readonly Mock<ILogger<CsvFileHandler>> _mockLogger;
        private readonly CsvFileHandler _csvFileHandler;
        private readonly string _testFilePath;

        public CsvFileHandlerTests()
        {
            _mockLogger = new Mock<ILogger<CsvFileHandler>>();
            _csvFileHandler = new CsvFileHandler(_mockLogger.Object);
            _testFilePath = Path.GetTempFileName();
        }

        [Fact]
        public void CanHandle_WithCsvExtension_ShouldReturnTrue()
        {
            // Arrange
            var filePath = "test.csv";

            // Act
            var result = _csvFileHandler.CanHandle(filePath);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanHandle_WithNonCsvExtension_ShouldReturnFalse()
        {
            // Arrange
            var filePath = "test.json";

            // Act
            var result = _csvFileHandler.CanHandle(filePath);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ReadDisputesAsync_WithValidCsvFile_ShouldReturnDisputes()
        {
            // Arrange
            var csvContent = @"DisputeId,TransactionId,Amount,Currency,Status,Reason
case_001,txn_001,100.00,USD,Open,Fraud
case_002,txn_002,150.00,EUR,Won,Product Not Received";

            await File.WriteAllTextAsync(_testFilePath, csvContent);

            // Act
            var result = await _csvFileHandler.ReadDisputesAsync(_testFilePath);

            // Assert
            var disputes = result.ToList();
            disputes.Should().HaveCount(2);

            disputes[0].DisputeId.Should().Be("case_001");
            disputes[0].Amount.Should().Be(100.00m);
            disputes[0].Currency.Should().Be("USD");

            disputes[1].DisputeId.Should().Be("case_002");
            disputes[1].Amount.Should().Be(150.00m);
            disputes[1].Currency.Should().Be("EUR");
        }

        [Fact]
        public async Task WriteReconciliationResultAsync_WithCsvExtension_ShouldWriteCsvFile()
        {
            // Arrange
            var result = new ReconcileResult
            {
                Discrepancies =
                [
                    new()
                    {
                        DisputeId = "case_001",
                        Type = DiscrepancyType.MissingInInternal,
                        Description = "Test discrepancy",
                        Severity = SeverityLevel.High
                    }
                ]
            };

            var outputPath = Path.ChangeExtension(_testFilePath, ".csv");

            // Act
            await _csvFileHandler.WriteReconcileResultAsync(result, outputPath);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().Contain("case_001");
            content.Should().Contain("MissingInInternal");
        }

        internal void Dispose()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }
    }
}
