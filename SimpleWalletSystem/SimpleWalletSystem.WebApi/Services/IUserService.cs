using System.Data;

namespace SimpleWalletSystem.WebApi.Services
{
    public interface IUserService
    {
        long RegisterUser(User user);  
        DataSet GetUserDataSetByAccountNumber(long accountNumber);
    }
}