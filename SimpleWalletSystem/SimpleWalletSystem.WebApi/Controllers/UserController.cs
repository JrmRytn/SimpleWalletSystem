using SimpleWalletSystem.WebApi.ViewModel;
using System.Dynamic;

namespace SimpleWalletSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
       private readonly IUserService _userService;
       private readonly IMapper _mapper;
       private readonly ITransactionService _transactionService;
        
        public UserController(IUserService userService, IMapper mapper, ITransactionService transactionService)
        {
            _userService = userService;
            _mapper = mapper;
            _transactionService = transactionService;
        }

        [HttpPost("Register")]
        [ProducesResponseType(typeof(Result<string>), 200)]
        public IActionResult RegisterUser([FromBody] RegisterUserDto dto)
        {
            dynamic returnObject = new ExpandoObject();
            Result<object> result = new();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                // Filter out properties with empty validation errors
                var filteredErrors = errors.Where(kvp => kvp.Value != null && kvp.Value.Length > 0)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

               

                Result<Dictionary<string, string[]>> resultValidation = new()
                {
                    ResponseCode = 400,
                    Message = "Field validation failed",
                    Data = filteredErrors
                };
              
                return BadRequest(resultValidation);  
            }
            else
            {
                try
                {
                 
                    var newUserRequest = _mapper.Map<User>(dto);
                    var createUserResult =  _userService.RegisterUser(newUserRequest);
                    
                    result.ResponseCode = 200;
                    result.Message = $"User register successfully.";
                    returnObject.AcountNumber = newUserRequest.AccountNumber;
                    result.Data = returnObject;
                      
                    return Ok(result); 
                     
                }
                catch(ConflictException ex)
                {
                    result.ResponseCode = 409;
                    result.Message = "Conflict exception error.";
                    returnObject.Error = ex.Message;
                    result.Data = returnObject;
                    return Conflict(result);
                }
                catch(BaseException ex)
                {
                    result.ResponseCode = 500;
                    result.Message = "Internal error.";
                    returnObject.Error = ex.Message;
                    return StatusCode(500, result);
                } 
                
            }
        }

        [HttpPost("Transaction")]
        [ProducesResponseType(typeof(Result<string>), 200)]
        public IActionResult Transaction([FromBody] TransactionDto dto)
        {
            Result<string> result = new();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                // Filter out properties with empty validation errors
                var filteredErrors = errors.Where(kvp => kvp.Value != null && kvp.Value.Length > 0)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                 

                Result<Dictionary<string, string[]>> resultValidation = new()
                {
                    ResponseCode = 400,
                    Message = "Field validation failed.",
                    Data = filteredErrors
                };

                return BadRequest(resultValidation);
            }
            else
            {
                
                try
                {
                    var successMessage = "";
                    if(dto.TransactionType == Enums.TransactionType.Transfer)
                    {
                        _transactionService.Transfer(dto.AccountNumber, dto.DestinationAccountNumber, dto.Amount);
                        successMessage = "Transfer";
                    }
                    else
                    {
                        _transactionService.WithdrawOrDeposit(dto.AccountNumber, dto.Amount, dto.TransactionType);
                        if((short)dto.TransactionType == 0 )
                            successMessage = "Deposit";
                        if ((short)dto.TransactionType == 1)
                            successMessage = "Withdraw";
                    }
                    
                    
                    result.ResponseCode = 200;
                    result.Message = $"{successMessage} successfully." ;
                    return Ok(result);

                }
                catch (NotFoundException ex)
                {
                    result.ResponseCode = 400;
                    result.Message = ex.Message;
                    return BadRequest(result);
                }
                catch (InsufficientBalanceException ex)
                {
                    result.ResponseCode = 400;
                    result.Message = ex.Message;
                    return BadRequest(result);
                }
                catch (TransferFailedException ex)
                {
                    result.ResponseCode = 400;
                    result.Message = ex.Message;
                    return BadRequest(result);
                } 
                catch (ConcurrencyException ex)
                {
                    result.ResponseCode = 400;
                    result.Message = ex.Message;
                    return BadRequest(result);
                }
                catch (BaseException ex)
                {
                    result.ResponseCode = 500;
                    result.Message = "Internal error.";
                    result.Data = ex.Message.ToString();
                    return StatusCode(500, result);
                } 
            }
        }

        [HttpGet("GetTransactionHistory")]
        public IActionResult GetTransactionHistory([FromQuery] long accountNumber)
        {
            Result<List<TransactionHistory>> result = new();
            try
            {
                if(accountNumber  == 0)
                {
                    Result<Dictionary<string, string[]>> resultValidation = new()
                    {
                        ResponseCode = 400,
                        Message = "Field validation failed.",
                        Data = new Dictionary<string, string[]>
                        {
                            { "accountNumber", new string[] { "Account number is required." } }
                        }
                    };

                    return BadRequest(resultValidation);
                }
                var dataResult = _transactionService.GetTransactionHistory(accountNumber);
                result.ResponseCode = 200;
                result.Message = "Data fetch successfully";
                result.Data = dataResult;
                return Ok(result);

            }catch (NotFoundException ex)
            {
                result.ResponseCode = 400;
                result.Message = ex.Message;
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                result.ResponseCode = 500;
                result.Message = "Internal error." + ex.Message.ToString(); 
                return StatusCode(500, result);
            }
        }

    }
}
