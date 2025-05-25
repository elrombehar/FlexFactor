using DisputeReconsile.Exceptions;
using DisputeReconsile.Interfaces;
using DisputeReconsile.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DisputeReconsile.Infra.FileHandlers
{
    public class JsonFileHandler(ILogger<JsonFileHandler> logger) : FileWriter(logger), IFileHandler
    {
        private readonly ILogger<JsonFileHandler> _logger = logger;

        public bool CanHandle(string filePath)
            => Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase);
        
        public async Task<IEnumerable<Dispute>> ReadDisputesAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Reading JSON file: {FilePath}", filePath);

                var json = await File.ReadAllTextAsync(filePath);
                var disputes = JsonConvert.DeserializeObject<List<Dispute>>(json) ?? new List<Dispute>();

                _logger.LogInformation("Successfully read {Count} disputes from JSON", disputes.Count);
                return disputes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading JSON file: {FilePath}", filePath);
                throw new FileProcessException($"Failed to read JSON file: {filePath}", ex);
            }
        }
    }
}
