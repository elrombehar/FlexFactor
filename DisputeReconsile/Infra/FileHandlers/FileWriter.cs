using System.Globalization;
using CsvHelper;
using DisputeReconsile.Exceptions;
using DisputeReconsile.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DisputeReconsile.Infra.FileHandlers
{
    public class FileWriter(ILogger<FileWriter> logger)
    {
        private readonly ILogger<FileWriter> _logger = logger;

        public async Task WriteReconcileResultAsync(ReconcileResult result, string outputPath)
        {
            try
            {
                _logger.LogInformation("Writing reconciliation result to: {OutputPath}", outputPath);

                var extension = Path.GetExtension(outputPath).ToLowerInvariant();

                switch (extension)
                {
                    case ".csv":
                        await WriteCsvResultAsync(result, outputPath);
                        break;
                    case ".json":
                        await WriteJsonResultAsync(result, outputPath);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported output format: {extension}");
                }

                _logger.LogInformation("Successfully wrote reconciliation result");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing reconciliation result: {OutputPath}", outputPath);
                throw new FileProcessException($"Failed to write reconciliation result: {outputPath}", ex);
            }
        }

        private static async Task WriteCsvResultAsync(ReconcileResult result, string outputPath)
        {
            using var writer = new StreamWriter(outputPath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            await csv.WriteRecordsAsync(result.Discrepancies.Select(d => new
            {
                d.DisputeId,
                Type = d.Type.ToString(),
                d.Description,
                Severity = d.Severity.ToString(),
                ExternalAmount = d.ExternalDispute?.Amount,
                ExternalStatus = d.ExternalDispute?.Status,
                InternalAmount = d.InternalDispute?.Amount,
                InternalStatus = d.InternalDispute?.Status
            }));
        }

        private static async Task WriteJsonResultAsync(ReconcileResult result, string outputPath)
        {
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            await File.WriteAllTextAsync(outputPath, json);
        }
    }
}
