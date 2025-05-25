using DisputeReconsile.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DisputeReconsile.Infra.FileHandlers
{
    public class FileHandlerFactory(IServiceProvider serviceProvider) : IFileHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public IFileHandler? GetFileHandler(string filePath)
        {
            var handlers = new IFileHandler?[]
            {
                _serviceProvider.GetService<CsvFileHandler>(),
                _serviceProvider.GetService<JsonFileHandler>(),
                _serviceProvider.GetService<XmlFileHandler>()
            };

            return handlers.FirstOrDefault(h => h?.CanHandle(filePath) == true);
        }
    }
}
