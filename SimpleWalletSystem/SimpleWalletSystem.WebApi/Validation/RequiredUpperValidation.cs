namespace SimpleWalletSystem.WebApi.Validation;
public class RequiredUpperValidation : ValidationAttribute
{
    public RequiredUpperValidation()
    {
        ErrorMessage = "{0} must have at least 1 capital letter.";
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessage, name);
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string propertyValue)
        {
            if (!propertyValue.Any(char.IsUpper))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }
        return ValidationResult.Success;
    }
}
