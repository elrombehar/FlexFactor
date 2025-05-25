namespace DisputeReconsile.Exceptions
{
    public class FileProcessException : DisputeReconcileException
    {
        public FileProcessException(string message) : base(message) { }

        public FileProcessException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
