using SimpleWalletSystem.WebApi.Enums;
using SimpleWalletSystem.WebApi.ViewModel;

namespace SimpleWalletSystem.WebApi.Services
{
    public interface ITransactionService
    {
        void WithdrawOrDeposit(long accountNumber, decimal amount, TransactionType transactionType);
        void Transfer(long accountNumberSource, long accountNumberDestination, decimal amount);
        List<TransactionHistory> GetTransactionHistory(long accountNumber);
    }
}