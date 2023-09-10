using SimpleWalletSystem.WebApi.Exceptions;

namespace SimpleWalletSystem.Test
{

    [TestClass]
    public class UserServiceTest
    {

        [TestMethod]
        public void RegisterUser_SuccessfulRegistration_ReturnsAccountNumber()
        {
            // Arrange
            var user = new User
            {
                UserId = Guid.NewGuid(),
                LoginName = "testuser",
                Password = "P@ssw0rd@1", 
                RegisterDate = DateTime.UtcNow
            };

            var connectionStringsMock = new Mock<IOptions<ConnectionStrings>>();
            connectionStringsMock.Setup(x => x.Value).Returns(new ConnectionStrings { Default = Connection.DEFAULT });

            var userService = new UserService(connectionStringsMock.Object);

            // Act
            long accountNumber = userService.RegisterUser(user);

            // Assert
            Assert.IsTrue(accountNumber > 0);
        }

        [TestMethod]
        public void RegisterUser_DuplicateLoginName_ThrowsConflictException()
        {
            // Arrange
            var user1 = new User
            {
                UserId = Guid.NewGuid(),
                LoginName = "testuser1",
                Password = "P@ssw0rd@4", 
                RegisterDate = DateTime.UtcNow
            };

            var user2 = new User
            {
                UserId = Guid.NewGuid(),
                LoginName = "testuser1", // Duplicate login name
                Password = "P@ssw0rd@4", 
                RegisterDate = DateTime.UtcNow
            };

            var connectionStringsMock = new Mock<IOptions<ConnectionStrings>>();
            connectionStringsMock.Setup(x => x.Value).Returns(new ConnectionStrings { Default = Connection.DEFAULT });

            var userService = new UserService(connectionStringsMock.Object);

            // Act
            userService.RegisterUser(user1);

            // Assert
            Assert.ThrowsException<ConflictException>(() => userService.RegisterUser(user2));
        } 

    }
}