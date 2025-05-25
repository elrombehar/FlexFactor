using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DisputeReconsile.Interfaces;
using DisputeReconsile.Services;
using DisputeReconsile.Infra.Repos;
using DisputeReconsile.Infra.FileHandlers;
using DisputeReconsile.Infra.ExchangeRate;
using DisputeReconsile.Infra.Alerts;
using DisputeReconsile.Models;

namespace DisputeReconsile
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Dispute Reconcile");

            var inputFileOption = new Option<string>(
            name: "--input",
            description: "Path to the input dispute report file (CSV, JSON, or XML)")
            {
                IsRequired = true
            };
            inputFileOption.AddAlias("-i");

            var outputFileOption = new Option<string>(
                name: "--output",
                description: "Path to the output reconciliation report file (CSV or JSON)")
            {
                IsRequired = true
            };
            outputFileOption.AddAlias("-o");

            var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose logging")
            {
                IsRequired = false
            };

            rootCommand.AddOption(inputFileOption);
            rootCommand.AddOption(outputFileOption);
            rootCommand.AddOption(verboseOption);

            rootCommand.SetHandler(async (inputFile, outputFile, verbose) =>
            {
                try
                {
                    await RunReconcileAsync(inputFile, outputFile, verbose);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error: {ex.Message}");
                    Environment.Exit(1);
                }
            }, inputFileOption, outputFileOption, verboseOption);

            // Parse and invoke the command
            return await rootCommand.InvokeAsync(args);
        }

        private static IHostBuilder CreateHostBuilder(bool verbose)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IReconcileService, ReconcileService>();
                    services.AddSingleton<IDisputeRepo, InMemDisputeRepo>();
                    services.AddTransient<CsvFileHandler>();
                    services.AddTransient<JsonFileHandler>();
                    services.AddTransient<XmlFileHandler>();
                    services.AddSingleton<IFileHandlerFactory, FileHandlerFactory>();
                    services.AddSingleton<IExchangeRateService, ExchangeRateService>();
                    services.AddSingleton<IAlertService, AlertService>();
                });
        }

        private static async Task RunReconcileAsync(string inputFile, string outputFile, bool verbose)
        {
            // Build the service host
            var host = CreateHostBuilder(verbose).Build();

            try
            {
                using var scope = host.Services.CreateScope();
                var services = scope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Starting Dispute Reconcile");
                logger.LogInformation("Input file: {InputFile}", inputFile);
                logger.LogInformation("Output file: {OutputFile}", outputFile);
                
                if (!File.Exists(inputFile))
                {
                    throw new FileNotFoundException($"Input file not found: {inputFile}");
                }

                var fileHandlerFactory = services.GetRequiredService<IFileHandlerFactory>();
                var reconciliationService = services.GetRequiredService<IReconcileService>();
                var disputeRepository = services.GetRequiredService<IDisputeRepo>();

                await ProcessReconcileAsync(
                    inputFile,
                    outputFile,
                    fileHandlerFactory,
                    reconciliationService,
                    disputeRepository,
                    logger);

                logger.LogInformation("Dispute reconcile completed successfully");
                Console.WriteLine("Reconcile completed successfully!");
                Console.WriteLine($"Reconcile results written to: {outputFile}");
            }
            catch (Exception ex)
            {
                var logger = host.Services.GetService<ILogger<Program>>();
                logger?.LogError(ex, "Fatal error during reconcile process");
                Console.WriteLine($"Fatal error: {ex.Message}");
                throw;
            }
        }

        private static async Task ProcessReconcileAsync(string inputFile, string outputFile, IFileHandlerFactory fileHandlerFactory,
                                                             IReconcileService reconciliationService, IDisputeRepo disputeRepository, ILogger logger)
        {
            var fileHandler = fileHandlerFactory.GetFileHandler(inputFile);
            if (fileHandler == null)
            {
                throw new NotSupportedException($"File format not supported: {Path.GetExtension(inputFile)}");
            }

            Console.WriteLine("Fetching external disputes");

            var externalDisputes = await fileHandler.ReadDisputesAsync(inputFile);
            var externalList = externalDisputes.ToList();

            Console.WriteLine($"Found {externalList.Count} external disputes");
            Console.WriteLine("Fetching internal disputes...");
            
            var internalDisputes = await disputeRepository.GetAllDisputesAsync();
            var internalList = internalDisputes.ToList();

            Console.WriteLine($"Found {internalList.Count} internal disputes");

            Console.WriteLine("Processing...");
            var reconciliationResult = await reconciliationService.ReconcileDisputesAsync(externalList, internalList);

            reconciliationResult.DisplaySummary(); 

            Console.WriteLine("Generating report...");
            await fileHandler.WriteReconcileResultAsync(reconciliationResult, outputFile);
        }

        
    }
}
