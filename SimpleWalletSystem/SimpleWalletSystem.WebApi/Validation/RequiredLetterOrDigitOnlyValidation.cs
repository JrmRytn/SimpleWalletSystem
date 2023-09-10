namespace SimpleWalletSystem.WebApi.Validation;

public class RequiredLetterOrDigitOnlyValidation : ValidationAttribute 
{
    public RequiredLetterOrDigitOnlyValidation()
    {
        ErrorMessage = "{0} must must contain only letters and numbers.";
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessage, name);
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string propertyValue)
        {
            if (!propertyValue.All(char.IsLetterOrDigit))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }
        return ValidationResult.Success;
    }
     
}
