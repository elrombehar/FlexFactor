using System.Collections.Concurrent;
using DisputeReconsile.Interfaces;
using DisputeReconsile.Models;
using Microsoft.Extensions.Logging;

namespace DisputeReconsile.Services
{
    public class ReconcileService(IExchangeRateService exchangeRateService, IAlertService alertService,
                            ILogger<ReconcileService> logger) : IReconcileService
    {
        private readonly IExchangeRateService _exchangeRateService = exchangeRateService;
        private readonly IAlertService _alertService = alertService;
        private readonly ILogger<ReconcileService> _logger = logger;

        public async Task<ReconcileResult> ReconcileDisputesAsync(IEnumerable<Dispute> externalDisputes, IEnumerable<Dispute> internalDisputes)
        {
            _logger.LogInformation("Starting dispute reconcile process");

            var externalList = externalDisputes.ToList();
            var internalList = internalDisputes.ToList();

            var result = new ReconcileResult();
            var discrepancies = new ConcurrentBag<DisputeDiscrepancy>();

            var externalLookup = externalList.ToDictionary(d => d.DisputeId, d => d);
            var internalLookup = internalList.ToDictionary(d => d.DisputeId, d => d);

            Parallel.Invoke(
                        () => FindMissingDisputes(externalLookup, internalLookup, DiscrepancyType.MissingInInternal, discrepancies),
                        () => FindMissingDisputes(internalLookup, externalLookup, DiscrepancyType.MissingInExternal, discrepancies)
            );

            //await CompareMatchingDisputesAsync(externalLookup, internalLookup, discrepancies);
            await ParallelCompareMatchingDisputesAsync(externalLookup, internalLookup, discrepancies);

            result.Discrepancies = [.. discrepancies]; // ToList()
            result.Summary = GenerateSummary(externalList, internalList, result.Discrepancies);

            var highSeverityDiscrepancies = discrepancies
                .Where(d => d.Severity >= SeverityLevel.High)
                .ToList();

            if (highSeverityDiscrepancies.Count > 0)
            {
                await _alertService.SendHighSeverityDiscrepancyAlertAsync(highSeverityDiscrepancies);
            }

            _logger.LogInformation("Reconciliation completed. Found {DiscrepancyCount} discrepancies, {HighSeverityCount} high-severity", discrepancies.Count, highSeverityDiscrepancies.Count);

            return result;
        }

        private static void FindMissingDisputes(Dictionary<string, Dispute> primaryLookup, Dictionary<string, Dispute> secondaryLookup,
                                         DiscrepancyType discrepancyType, ConcurrentBag<DisputeDiscrepancy> discrepancies)
        {
            foreach (var (disputeId, primaryDispute) in primaryLookup)
            {
                if (!secondaryLookup.ContainsKey(disputeId))
                {
                    var severity = discrepancyType == DiscrepancyType.MissingInInternal
                        ? SeverityService.DetermineSeverity(primaryDispute, null, discrepancyType)
                        : SeverityService.DetermineSeverity(null, primaryDispute, discrepancyType);

                    discrepancies.Add(new DisputeDiscrepancy
                    {
                        DisputeId = disputeId,
                        Type = discrepancyType,
                        Description = discrepancyType == DiscrepancyType.MissingInInternal
                            ? $"Dispute {disputeId} exists in external report but missing in internal records"
                            : $"Dispute {disputeId} exists in internal records but missing in external report",
                        ExternalDispute = discrepancyType == DiscrepancyType.MissingInInternal ? primaryDispute : null,
                        InternalDispute = discrepancyType == DiscrepancyType.MissingInExternal ? primaryDispute : null,
                        Severity = severity
                    });
                }
            }
        }

        private async Task CompareMatchingDisputesAsync(Dictionary<string, Dispute> externalLookup, Dictionary<string, Dispute> internalLookup,
                                                        ConcurrentBag<DisputeDiscrepancy> discrepancies)
        {
            foreach (var (disputeId, externalDispute) in externalLookup)
            {
                if (internalLookup.TryGetValue(disputeId, out var internalDispute))
                {
                    await CompareDisputeDetailsAsync(externalDispute, internalDispute, discrepancies);
                }
            }
        }

        private async Task ParallelCompareMatchingDisputesAsync(Dictionary<string, Dispute> externalLookup,
                                                           Dictionary<string, Dispute> internalLookup,
                                                           ConcurrentBag<DisputeDiscrepancy> discrepancies)
        {
            var matchingPairs = externalLookup
                                .Where(kvp => internalLookup.ContainsKey(kvp.Key))
                                .Select(kvp => (External: kvp.Value, Internal: internalLookup[kvp.Key]))
                                .ToList();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1 // Leave one core free for responsiveness
            };

            await Parallel.ForEachAsync(matchingPairs, options, async (pair, cancellationToken) =>
            {
                await CompareDisputeDetailsAsync(pair.External, pair.Internal, discrepancies);
            });
        }


        private async Task CompareDisputeDetailsAsync(Dispute externalDispute,  Dispute internalDispute, ConcurrentBag<DisputeDiscrepancy> discrepancies)
        {
            if (!string.Equals(externalDispute.Status, internalDispute.Status, StringComparison.OrdinalIgnoreCase))
            {
                var severity = SeverityService.DetermineStatusMismatchSeverity(externalDispute.Status, internalDispute.Status);
            
                discrepancies.Add(new DisputeDiscrepancy
                {
                    DisputeId = externalDispute.DisputeId,
                    Type = DiscrepancyType.StatusMismatch,
                    Description = $"Status mismatch: External='{externalDispute.Status}', Internal='{internalDispute.Status}'",
                    ExternalDispute = externalDispute,
                    InternalDispute = internalDispute,
                    Severity = severity
                }); 
            }

            await CompareAmountsAsync(externalDispute, internalDispute, discrepancies);

            if (!string.Equals(externalDispute.Currency, internalDispute.Currency, StringComparison.OrdinalIgnoreCase))
            {
                discrepancies.Add(new DisputeDiscrepancy
                {
                    DisputeId = externalDispute.DisputeId,
                    Type = DiscrepancyType.CurrencyMismatch,
                    Description = $"Currency mismatch: External='{externalDispute.Currency}', Internal='{internalDispute.Currency}'",
                    ExternalDispute = externalDispute,
                    InternalDispute = internalDispute,
                    Severity = SeverityLevel.Medium
                });
            }

            if (!string.Equals(externalDispute.Reason, internalDispute.Reason, StringComparison.OrdinalIgnoreCase))
            {
                discrepancies.Add(new DisputeDiscrepancy
                {
                    DisputeId = externalDispute.DisputeId,
                    Type = DiscrepancyType.ReasonMismatch,
                    Description = $"Reason mismatch: External='{externalDispute.Reason}', Internal='{internalDispute.Reason}'",
                    ExternalDispute = externalDispute,
                    InternalDispute = internalDispute,
                    Severity = SeverityLevel.Low
                });
            }
        }

        private async Task CompareAmountsAsync(Dispute externalDispute, Dispute internalDispute, ConcurrentBag<DisputeDiscrepancy> discrepancies)
        {
            decimal externalAmount = externalDispute.Amount;
            decimal internalAmount = internalDispute.Amount;

            const decimal tolerance = 0.01m; // 1 cent tolerance for rounding differences - can be set as external config

            if (!string.Equals(externalDispute.Currency, internalDispute.Currency, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    externalAmount = await _exchangeRateService.ConvertAmountAsync(externalDispute.Amount,
                        externalDispute.Currency, internalDispute.Currency);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to convert currency for dispute {DisputeId}", externalDispute.DisputeId);
                    return; // Skip amount comparison if currency conversion fails
                }

                if (Math.Abs(externalAmount - internalAmount) > tolerance)
                {
                    var severity = SeverityService.DetermineAmountMismatchSeverity(Math.Abs(externalAmount - internalAmount));

                    discrepancies.Add(new DisputeDiscrepancy
                    {
                        DisputeId = externalDispute.DisputeId,
                        Type = DiscrepancyType.AmountMismatch,
                        Description = $"Amount mismatch: External={externalAmount:C}, Internal={internalAmount:C}",
                        ExternalDispute = externalDispute,
                        InternalDispute = internalDispute,
                        Severity = severity
                    });
                }
            }
            else
            {
                if (Math.Abs(externalAmount - internalAmount) > tolerance) 
                {
                    var severity = SeverityService.DetermineAmountMismatchSeverity(Math.Abs(externalAmount - internalAmount));
                    discrepancies.Add(new DisputeDiscrepancy
                    {
                        DisputeId = externalDispute.DisputeId,
                        Type = DiscrepancyType.AmountMismatch,
                        Description = $"Amount mismatch: External={externalAmount:C}, Internal={internalAmount:C}",
                        ExternalDispute = externalDispute,
                        InternalDispute = internalDispute,
                        Severity = severity
                    });
                }
            }
        }
        private static ReconcileSummary GenerateSummary(List<Dispute> externalDisputes, List<Dispute> internalDisputes, 
                                                 List<DisputeDiscrepancy> discrepancies)
            => new()
            {
                TotalExternalRecords = externalDisputes.Count,
                TotalInternalRecords = internalDisputes.Count,
                TotalDiscrepancies = discrepancies.Count,
                MissingInInternal = discrepancies.Count(d => d.Type == DiscrepancyType.MissingInInternal),
                MissingInExternal = discrepancies.Count(d => d.Type == DiscrepancyType.MissingInExternal),
                StatusMismatches = discrepancies.Count(d => d.Type == DiscrepancyType.StatusMismatch),
                AmountMismatches = discrepancies.Count(d => d.Type == DiscrepancyType.AmountMismatch),
                HighSeverityDiscrepancies = discrepancies.Count(d => d.Severity >= SeverityLevel.High)
            };
    }
}


