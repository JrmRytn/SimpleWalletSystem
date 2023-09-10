using SimpleWalletSystem.WebApi.Enums;
using System.Text.RegularExpressions;

namespace SimpleWalletSystem.WebApi.Dto;

public class TransactionDto : IValidatableObject
{

    [RegularExpression(@"^\d{12}$", ErrorMessage = "Account number must be 12 digits.")]
    public long AccountNumber { get; set; }
    [ConditionalRegex(@"^\d{12}$", ErrorMessage = "Destination account number  number must be 12 digits.")]
    public long DestinationAccountNumber { get; set; }
    [Required]
    public TransactionType TransactionType { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    { 
        if (TransactionType == TransactionType.Transfer)
        {
            if(DestinationAccountNumber == 0)
                yield return new ValidationResult("Destination account number is required for transfer transaction.", new[] { nameof(DestinationAccountNumber) });
        }

        if (Amount <= 0)
            yield return new ValidationResult("Amount must be greater than zero.", new[] { nameof(Amount) });
    }
}
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class ConditionalRegexAttribute : ValidationAttribute
{
    public string Pattern { get; }

    public ConditionalRegexAttribute(string pattern)
    {
        Pattern = pattern;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var transactionDto = (TransactionDto)validationContext.ObjectInstance;

        if (transactionDto.TransactionType == TransactionType.Transfer)
        {
            var regex = new Regex(Pattern);

            if (!regex.IsMatch(value.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
} 
 
