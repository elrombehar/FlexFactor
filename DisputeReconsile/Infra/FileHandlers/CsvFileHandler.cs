using System.Globalization;
using DisputeReconsile.Interfaces;
using DisputeReconsile.Models;
using Microsoft.Extensions.Logging;
using CsvHelper;
using CsvHelper.Configuration;
using DisputeReconsile.Exceptions;

namespace DisputeReconsile.Infra.FileHandlers
{
    public class CsvFileHandler(ILogger<CsvFileHandler> logger) : FileWriter(logger), IFileHandler
    {
        private readonly ILogger<CsvFileHandler> _logger = logger;

        public bool CanHandle(string filePath)
            => Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);
        

        public async Task<IEnumerable<Dispute>> ReadDisputesAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Reading CSV file: {FilePath}", filePath);

                var disputes = new List<Dispute>();

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                });

                await foreach (var record in csv.GetRecordsAsync<DisputeCsvRecord>())
                {
                    disputes.Add(MapCsvRecordToDispute(record));
                }

                _logger.LogInformation("Successfully read {Count} disputes from CSV", disputes.Count);
                return disputes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading CSV file: {FilePath}", filePath);
                throw new FileProcessException($"Failed to read CSV file: {filePath}", ex);
            }
        }

        private static Dispute MapCsvRecordToDispute(DisputeCsvRecord record)
            => new()
            {
                DisputeId = record.DisputeId ?? string.Empty,
                TransactionId = record.TransactionId ?? string.Empty,
                Amount = record.Amount,
                Currency = record.Currency ?? "USD",
                Status = record.Status ?? string.Empty,
                Reason = record.Reason ?? string.Empty
            };
    }
}
