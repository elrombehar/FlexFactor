namespace DisputeReconsile.Exceptions
{
    public class DisputeReconcileException : Exception
    {
        public DisputeReconcileException(string message) : base(message) { }

        public DisputeReconcileException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
