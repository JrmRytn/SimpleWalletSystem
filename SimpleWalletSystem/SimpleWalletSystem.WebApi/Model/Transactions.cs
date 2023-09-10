using SimpleWalletSystem.WebApi.Enums;

namespace SimpleWalletSystem.WebApi.Model;

public class Transactions
{
    public Transactions()
    {
        
    }
    public Transactions(Guid transactionId) : this()
    {
        TransactionId = transactionId;
    }

    public Transactions(Guid transactionId, long accountNumberSource, TransactionType transactionType,decimal amount, long accountNumberDestination, decimal endBalance) : this(transactionId)
    {
        AccountNumberSource = accountNumberSource;
        TransactionType = transactionType;
        Amount = amount;
        AccountNumberDestination = accountNumberDestination;
        DateOfTransaction = DateTime.Now;
        EndBalance = endBalance;
    } 
    public Guid TransactionId { get; set; }
    public long AccountNumberSource { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public long AccountNumberDestination { get; set; } // For Transfer
    public DateTime DateOfTransaction { get; set; }
    public decimal EndBalance { get; set; }

}
