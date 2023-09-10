namespace SimpleWalletSystem.WebApi.Exceptions;

[Serializable]
public class TransferFailedException : Exception
{
    public TransferFailedException()
    {
    }

    public TransferFailedException(string message) : base(message)
    {
    }

    public TransferFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }

}