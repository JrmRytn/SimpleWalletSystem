namespace SimpleWalletSystem.WebApi.Validation;

public class RequiredDigitValidation : ValidationAttribute
{
    public RequiredDigitValidation()
    {
        ErrorMessage = "{0} must have at least 1 digit.";
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessage, name);
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string propertyValue)
        {
            if (!propertyValue.Any(char.IsDigit))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }
        return ValidationResult.Success;
    }
     
}
