namespace SimpleWalletSystem.WebApi.Exceptions;

[Serializable]
public class UpdateFailedException : Exception
{
    public UpdateFailedException()
    {
        
    }
    public UpdateFailedException(string message) : base(message)
    {
        
    }
    public UpdateFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
