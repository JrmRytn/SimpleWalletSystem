namespace SimpleWalletSystem.WebApi.Common
{
    public class Result<T>
    {
        public int ResponseCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
