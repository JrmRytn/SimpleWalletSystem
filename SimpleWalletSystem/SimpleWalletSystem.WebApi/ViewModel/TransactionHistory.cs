using SimpleWalletSystem.WebApi.Enums;

namespace SimpleWalletSystem.WebApi.ViewModel
{
    public class TransactionHistory
    { 
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public long AccountNumber { get; set; }  
        public DateTime DateOfTransaction { get; set; }
        public decimal EndBalance { get; set; }
    }
}
