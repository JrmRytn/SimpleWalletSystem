using SimpleWalletSystem.WebApi.Enums;
using SimpleWalletSystem.WebApi.Exceptions;
using SimpleWalletSystem.WebApi.Services;
using System.Data;

namespace SimpleWalletSystem.Test
{

    [TestClass]
    public class TransactionServiceTest
    {

        private IUserService _userService;
        private Mock<IOptions<ConnectionStrings>> _connectionStringsMock;
        private TransactionService _transactionService;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize mocks and the transaction service
            
            _connectionStringsMock = new Mock<IOptions<ConnectionStrings>>();

            // Create an instance of ConnectionStrings with your connection details
            var connectionStrings = new ConnectionStrings
            {
                Default = Connection.DEFAULT
            };

            _connectionStringsMock.Setup(c => c.Value).Returns(connectionStrings);
            // Create an instance of UserService with the actual implementation
            _userService = new UserService(_connectionStringsMock.Object);
            _transactionService = new TransactionService(_userService, _connectionStringsMock.Object);
        }

        [TestMethod]
        public void WithdrawOrDeposit_SuccessfulWithdraw()
        {
            // Arrange
            long accountNumber = 100530748917;
            decimal initialBalance = 150;
            decimal withdrawAmount = 50;
            TransactionType transactionType = TransactionType.Withdraw;
             
            // Act
            _transactionService.WithdrawOrDeposit(accountNumber, withdrawAmount, transactionType);

            // Assert
             
            decimal expectedBalanceAfterWithdraw = initialBalance - withdrawAmount;

            // Verify the expected balance matches the actual balance in the data set
            var actualUserData = _userService.GetUserDataSetByAccountNumber(accountNumber);
            decimal actualBalanceAfterWithdraw = actualUserData.Tables["Users"].Rows[0].Field<decimal>("Balance");

            Assert.AreEqual(expectedBalanceAfterWithdraw, actualBalanceAfterWithdraw, "Balance after withdrawal does not match.");
        }

        [TestMethod]
        public void UnsuccessfulWithdraw_InsufficientBalance()
        {
            // Arrange
            long accountNumber = 100570868121; 
            decimal withdrawAmount = 100;
            TransactionType transactionType = TransactionType.Withdraw;
             
            // Act and Assert
            Assert.ThrowsException<InsufficientBalanceException>(() => _transactionService.WithdrawOrDeposit(accountNumber, withdrawAmount, transactionType));
        }
        [TestMethod]
        public void UnsuccessfulWithdraw_AccountNotFound()
        {
            // Arrange
            long accountNumber = 1005708681229;
            decimal withdrawAmount = 100;
            TransactionType transactionType = TransactionType.Withdraw;

            // Act and Assert
            Assert.ThrowsException<NotFoundException>(() => _transactionService.WithdrawOrDeposit(accountNumber, withdrawAmount, transactionType));
        }

        [TestMethod]
        public void WithdrawOrDeposit_SuccessfulDeposit()
        {
            // Arrange
            long accountNumber = 100570868121;
            decimal initialBalance = 0  ;
            decimal depositAmount = 50;
            TransactionType transactionType = TransactionType.Deposit;

            // Act
            _transactionService.WithdrawOrDeposit(accountNumber, depositAmount, transactionType);

            // Assert
             
            decimal expectedBalanceAfterDeposit = initialBalance + depositAmount;

            // Verify the expected balance matches the actual balance in the data set
            var actualUserData = _userService.GetUserDataSetByAccountNumber(accountNumber);
            decimal actualBalanceAfterDeposit = actualUserData.Tables["Users"].Rows[0].Field<decimal>("Balance");

            Assert.AreEqual(expectedBalanceAfterDeposit, actualBalanceAfterDeposit, "Balance after deposit does not match.");
        }

        [TestMethod]
        public void Transfer_Successful()
        {
            // Arrange
            long sourceAccountNumber = 100530748917;
            decimal sourceBalance = 100;
            long destinationAccountNumber = 100161570758;
            decimal destinationBalance = 0;
            decimal transferAmount = 50;
              
            // Act
            _transactionService.Transfer(sourceAccountNumber, destinationAccountNumber, transferAmount);

            // Assert
             
            decimal sourceExpectedBalanceAfterTransfer = sourceBalance - transferAmount;
            decimal destinationExpectedBalanceAfterTransfer = destinationBalance + transferAmount;

            // Verify the expected balance matches the actual balance in the data set
            var sourceActualUserData = _userService.GetUserDataSetByAccountNumber(sourceAccountNumber);
            decimal sourceActualBalanceAfterTransfer = sourceActualUserData.Tables["Users"].Rows[0].Field<decimal>("Balance");

            Assert.AreEqual(sourceExpectedBalanceAfterTransfer, sourceActualBalanceAfterTransfer, "Source account balance after transfer does not match.");

            var destinationActualUserData = _userService.GetUserDataSetByAccountNumber(destinationAccountNumber);
            decimal destinationActualBalanceAfterTransfer = destinationActualUserData.Tables["Users"].Rows[0].Field<decimal>("Balance");

            Assert.AreEqual(destinationExpectedBalanceAfterTransfer, destinationActualBalanceAfterTransfer, "Destination account balance after transfer does not match.");
        } 
    }
}
