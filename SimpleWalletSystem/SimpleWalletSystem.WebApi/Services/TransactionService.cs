using SimpleWalletSystem.WebApi.Enums;
using SimpleWalletSystem.WebApi.ViewModel;
using System.Data;

namespace SimpleWalletSystem.WebApi.Services;

public class TransactionService : ITransactionService
{
    private readonly IUserService _userService;
    private readonly ConnectionStrings _connectionStrings;

    public TransactionService(IUserService userService, IOptions<ConnectionStrings> connectionStringsOption)
    {
        _userService = userService;
        _connectionStrings = connectionStringsOption.Value;
        adapter = new SqlDataAdapter(); // Initialize the adapter in the constructor
    }
    private readonly SqlDataAdapter adapter;
    public decimal UpdateUserBalance(long accountNumber,decimal amount,TransactionType transactionType)
    {
        using SqlConnection connection = new(_connectionStrings.Default);
        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction();
        decimal newBalance;
        try
        {
           
            // Retrieve user data
            var userData = _userService.GetUserDataSetByAccountNumber(accountNumber);
            if (userData.Tables["Users"]?.Rows.Count == 0)
            { 
                throw new NotFoundException("Account not found.");
            }

            byte[] currentRowVersion = userData.Tables["Users"].Rows[0].Field<byte[]>("RowVersion");

            // Update the balance based on the transaction type

             newBalance = userData.Tables["Users"].Rows[0].Field<decimal>("Balance");

            if (transactionType == TransactionType.Deposit)
            {
                newBalance += amount;
            }
            else if (transactionType == TransactionType.Withdraw)
            {
                // Add logic to check if sufficient balance is available before deducting.
                if (newBalance >= amount)
                {
                    newBalance -= amount;
                }
                else
                {
                    throw new InsufficientBalanceException("Insufficient balance for withdrawal.");
                }
            }

            // Update the balance in the database
            // Note: Since I use RowVersion Datatype We don't need to increment the RowVersion manually; it will be automatically updated by the database
            adapter.UpdateCommand = new SqlCommand(
           "UPDATE Users SET Balance = @Balance WHERE AccountNumber = @AccountNumber AND RowVersion = @CurrentRowVersion",
           connection);

            adapter.UpdateCommand.Parameters.Add("@Balance", SqlDbType.Decimal).Value = newBalance;
            adapter.UpdateCommand.Parameters.Add("@AccountNumber", SqlDbType.BigInt).Value = accountNumber;
            adapter.UpdateCommand.Parameters.Add("@CurrentRowVersion", SqlDbType.Timestamp).Value = currentRowVersion;
            adapter.UpdateCommand.Transaction = transaction;

            int rowsAffected = adapter.UpdateCommand.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // Concurrency violation occurred, rollback transaction and handle appropriately 
                throw new ConcurrencyException("Concurrency violation during update.");
            }
            else
            {
                // Update was successful, commit the transaction
                transaction.Commit();
                return newBalance;
            }
        }
        catch (SqlException ex)
        {
            newBalance = 0;
            transaction.Rollback();
            throw new BaseException("Save Transactions failed: " + ex.Message);
        }
        catch (ConcurrencyException ex)
        {
            newBalance = 0;
            transaction.Rollback();
            throw new ConcurrencyException("Save Transactions failed: " + ex.Message);
        }
        catch (InsufficientBalanceException ex)
        {
            newBalance = 0;
            transaction.Rollback();
            throw new InsufficientBalanceException("Save Transactions failed: " + ex.Message);
        }
        catch (NotFoundException ex)
        {
            newBalance = 0;
            transaction.Rollback();
            throw new NotFoundException("Save Transactions failed: " + ex.Message);
        }

        catch (Exception ex)
        {
            newBalance = 0;
            transaction.Rollback();
            throw new BaseException("Save Transactions failed: " + ex.Message);
        }
        finally
        {
            transaction.Dispose();
            connection.Close();
        }
      
    }  

    
    public void WithdrawOrDeposit(long accountNumber, decimal amount,TransactionType transactionType)
    { 
        var newBalance = UpdateUserBalance(accountNumber, amount, transactionType);  
        
        if(newBalance > 0)
            SaveTransaction(accountNumber, transactionType, amount, 0, newBalance); 
    }

    public void Transfer(long accountNumberSource, long accountNumberDestination, decimal transferAmount)
    {
        using SqlConnection connection = new(_connectionStrings.Default);
        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            // First, withdraw the amount from the source account
            var newSourceBalance = UpdateUserBalance(accountNumberSource, transferAmount, TransactionType.Withdraw);
            if (newSourceBalance > 0)
                SaveTransaction(accountNumberSource, TransactionType.Transfer, -transferAmount, accountNumberDestination, newSourceBalance);
            // Then, deposit the same amount into the destination account
            var destinationNewBalance = UpdateUserBalance(accountNumberDestination, transferAmount, TransactionType.Deposit);
            if (destinationNewBalance > 0)
                SaveTransaction(accountNumberSource, TransactionType.Transfer, transferAmount, accountNumberDestination, destinationNewBalance);

            // Commit the transaction
            transaction.Commit();
        }
        catch (Exception ex)
        {
            // If any exception occurs during the transfer, roll back the transaction
            transaction.Rollback();
            throw new TransferFailedException("Transfer operation failed: " + ex.Message);
        }
    }

 
    private void SaveTransaction(long accountNumberSource,TransactionType transactionType,decimal amount, long accountNumberDestination,decimal endingBalance)
    {
        using SqlConnection connection = new(_connectionStrings.Default);
        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction();

        using SqlCommand saveTransactionCommand = new("INSERT INTO Transactions(TransactionId,AccountNumberSource,TransactionType,Amount,AccountNumberDestination,DateOfTransaction,EndBalance)" +
            "VALUES(@TransactionId,@AccountNumberSource,@TransactionType,@Amount,@AccountNumberDestination,@DateOfTransaction,@EndBalance)", connection);

        Transactions transactionHistory = new(Guid.NewGuid(), accountNumberSource, transactionType, amount, accountNumberDestination, endingBalance);
         
        saveTransactionCommand.Parameters.AddWithValue("@TransactionId", transactionHistory.TransactionId);
        saveTransactionCommand.Parameters.AddWithValue("@AccountNumberSource", transactionHistory.AccountNumberSource);
        saveTransactionCommand.Parameters.AddWithValue("@TransactionType", transactionHistory.TransactionType.ToString());
        saveTransactionCommand.Parameters.AddWithValue("@Amount", transactionHistory.Amount);
        saveTransactionCommand.Parameters.AddWithValue("@AccountNumberDestination",transactionHistory.AccountNumberDestination);
        saveTransactionCommand.Parameters.AddWithValue("@DateOfTransaction", transactionHistory.DateOfTransaction);
        saveTransactionCommand.Parameters.AddWithValue("@EndBalance", transactionHistory.EndBalance);

        saveTransactionCommand.Transaction = transaction;
        try
        {
            saveTransactionCommand.ExecuteNonQuery();
            transaction.Commit(); 
        }
        catch (SqlException ex)
        {
            transaction.Rollback();
            throw new BaseException("Save Transactions failed: " + ex.Message);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new BaseException("Save Transactions failed: " + ex.Message);
        }
        finally
        {
            transaction.Dispose();
            connection.Close();
        }
    }

    public List<TransactionHistory> GetTransactionHistory(long accountNumber)
    {
        using SqlConnection connection = new (_connectionStrings.Default);
        connection.Open();

        string query = @"
            SELECT 
                TransactionType,
                Amount,
                AccountNumber,
                DateOfTransaction,
                EndBalance
            FROM (
                SELECT TransactionType, Amount, AccountNumberDestination AS AccountNumber, DateOfTransaction, EndBalance
                FROM [Transactions] 
                WHERE AccountNumberDestination = @AccountNumber AND Amount > 0

                UNION ALL

                SELECT TransactionType, Amount, AccountNumberSource AS AccountNumber, DateOfTransaction, EndBalance
                FROM [Transactions] 
                WHERE AccountNumberSource = @AccountNumber AND TransactionType != 'Transfer'

                UNION ALL

                SELECT TransactionType, Amount, AccountNumberSource AS AccountNumber, DateOfTransaction, EndBalance
                FROM [Transactions] 
                WHERE AccountNumberSource = @AccountNumber AND TransactionType = 'Transfer' AND Amount < 0
            ) AS TransactionHistory
            ORDER BY DateOfTransaction DESC";

        using SqlCommand command = new (query, connection);
        command.Parameters.AddWithValue("@AccountNumber", accountNumber);

        using SqlDataReader reader = command.ExecuteReader();
        List<TransactionHistory> transactionHistory = new();

        while (reader.Read())
        {
            var transaction = new TransactionHistory
            {

                TransactionType = reader["TransactionType"].ToString() ?? "",
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                AccountNumber = reader.GetInt64(reader.GetOrdinal("AccountNumber")),
                DateOfTransaction = reader.GetDateTime(reader.GetOrdinal("DateOfTransaction")),
                EndBalance = reader.GetDecimal(reader.GetOrdinal("EndBalance"))
            };

            transactionHistory.Add(transaction);
        }

        if (transactionHistory.Count == 0)
            throw new NotFoundException("Account not found.");
        return transactionHistory ;
    }
}
