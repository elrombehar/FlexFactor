namespace DisputeReconsile.Interfaces
{
    internal interface IFileHandlerFactory
    {
        IFileHandler? GetFileHandler(string filePath);
    }
}
