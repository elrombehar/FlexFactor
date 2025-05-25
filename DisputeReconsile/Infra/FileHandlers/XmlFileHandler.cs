using DisputeReconsile.Interfaces;
using DisputeReconsile.Models;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using DisputeReconsile.Exceptions;

namespace DisputeReconsile.Infra.FileHandlers
{
    public class XmlFileHandler(ILogger<XmlFileHandler> logger) : FileWriter(logger), IFileHandler
    {
        private readonly ILogger<XmlFileHandler> _logger = logger;

        public bool CanHandle(string filePath)
            => Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase);
        
        public async Task<IEnumerable<Dispute>> ReadDisputesAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Reading XML file: {FilePath}", filePath);

                var xmlDoc = await Task.Run(() => XDocument.Load(filePath));
                var disputes = new List<Dispute>();

                var disputeElements = xmlDoc.Descendants("Dispute");

                foreach (var element in disputeElements)
                {
                    var dispute = new Dispute
                    {
                        DisputeId = element.Element("DisputeId")?.Value ?? string.Empty,
                        TransactionId = element.Element("TransactionId")?.Value ?? string.Empty,
                        Amount = decimal.TryParse(element.Element("Amount")?.Value, out var amount) ? amount : 0,
                        Currency = element.Element("Currency")?.Value ?? "USD",
                        Status = element.Element("Status")?.Value ?? string.Empty,
                        Reason = element.Element("Reason")?.Value ?? string.Empty
                    };

                    disputes.Add(dispute);
                }

                _logger.LogInformation("Successfully read {Count} disputes from XML", disputes.Count);
                return disputes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading XML file: {FilePath}", filePath);
                throw new FileProcessException($"Failed to read XML file: {filePath}", ex);
            }
        }
    }
}
