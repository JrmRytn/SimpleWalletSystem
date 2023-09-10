using System.Data;

namespace SimpleWalletSystem.WebApi.Services;

public class UserService : IUserService
{
    private readonly ConnectionStrings _connectionStrings;

    public UserService(IOptions<ConnectionStrings> connectionStringsOption)
    {
        _connectionStrings = connectionStringsOption.Value;
    } 
    public long RegisterUser(User user)
    {
        using SqlConnection connection = new(_connectionStrings.Default);
        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction();
        if (LoginNameExists(user.LoginName, connection, transaction))
        {
            throw new ConflictException("Login name already exists.");
        } 
        using SqlCommand registerUserCommand = new("INSERT INTO Users (UserId, LoginName, AccountNumber, Password, Balance, RegisterDate) " +
                                                                  "VALUES (@UserId, @LoginName, @AccountNumber, @Password, @Balance, @RegisterDate)", connection);
        // Generate a unique account number based on the user's GUID
        user.AccountNumber = GenerateUniqueAccountNumber(connection, transaction, user.UserId);
        user.Password = Util.Base64Encode(user.Password);

        registerUserCommand.Parameters.AddWithValue("@UserId", user.UserId);
        registerUserCommand.Parameters.AddWithValue("@LoginName", user.LoginName);
        registerUserCommand.Parameters.AddWithValue("@AccountNumber", user.AccountNumber);
        registerUserCommand.Parameters.AddWithValue("@Password", user.Password);
        registerUserCommand.Parameters.AddWithValue("@Balance", user.Balance);
        registerUserCommand.Parameters.AddWithValue("@RegisterDate", user.RegisterDate); 

        registerUserCommand.Transaction = transaction;
        try
        {
            registerUserCommand.ExecuteNonQuery();
            transaction.Commit();
            return user.AccountNumber;
        }
        catch (SqlException ex)
        {
            transaction.Rollback();
            throw new BaseException("Register failed - Sql Exception: " + ex.Message);
        }
        catch(Exception ex)
        {
            transaction.Rollback();
            throw new BaseException("Register failed - Exception: " + ex.Message);
        }
        finally
        {
            transaction.Dispose();
            connection.Close();
        }
    } 
    private static bool LoginNameExists(string loginName, SqlConnection connection, SqlTransaction transaction)
    {
        using SqlCommand checkCommand = new("SELECT COUNT(*) FROM Users WHERE LoginName = @LoginName", connection, transaction);
        checkCommand.Parameters.AddWithValue("@LoginName", loginName);
        int existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());
        return existingCount > 0;
    }
    private static bool AccountNumberExists(long accountNumber, SqlConnection connection, SqlTransaction transaction)
    {
        using SqlCommand checkCommand = new("SELECT COUNT(*) FROM Users WHERE AccountNumber = @AccountNumber", connection, transaction);
        checkCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
        long existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());
        return existingCount > 0;
    }
    private static long GenerateUniqueAccountNumber(SqlConnection connection, SqlTransaction transaction, Guid userId)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] userIdBytes = userId.ToByteArray();
        byte[] hashBytes = sha256.ComputeHash(userIdBytes);
        long accountNumber = BitConverter.ToInt32(hashBytes, 0);

        accountNumber = Math.Abs(accountNumber) % 899999999999 + 100000000000;

        while (AccountNumberExists(accountNumber, connection, transaction))
        {
            userIdBytes[0]++;
            hashBytes = sha256.ComputeHash(userIdBytes);
            accountNumber = BitConverter.ToInt32(hashBytes, 0);
            accountNumber = Math.Abs(accountNumber) % 899999999999 + 100000000000;
        }

        return accountNumber;
    }

    private SqlDataAdapter adapter;
    public DataSet GetUserDataSetByAccountNumber(long accountNumber)
    {

        DataSet dataSet = new ();
        adapter = new();

        using SqlConnection connection = new(_connectionStrings.Default);
        connection.Open();
        // Modify the SELECT statement to include a WHERE clause
        adapter.SelectCommand = new SqlCommand(
            "SELECT * FROM Users WHERE AccountNumber = @AccountNumber",
            connection);
        adapter.SelectCommand.Parameters.Add("@AccountNumber", SqlDbType.BigInt).Value = accountNumber;

        adapter.Fill(dataSet, "Users");
        return dataSet;
    }


    
}
