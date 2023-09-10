namespace SimpleWalletSystem.WebApi.Model;

public class User
{
    public Guid UserId { get; set; }
    public string LoginName { get; set; }
    public long AccountNumber { get; set; }
    public string Password { get; set; }
    public decimal Balance { get; set; }  
    public DateTime RegisterDate { get; set; } 
 

    public User()
    {
        
    }
    public User(Guid userId) : this()
    {
        UserId = userId;
    }

    public User(Guid userId, string loginName, string password) : this(userId)
    { 
        LoginName = loginName; 
        Password = password;
        Balance = 0;
        RegisterDate = DateTime.Now.Date;  
    } 
}
