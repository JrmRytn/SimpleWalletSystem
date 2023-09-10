namespace SimpleWalletSystem.WebApi.Validation;

public class RequiredSpecialCharacterValidation : ValidationAttribute
{
    public RequiredSpecialCharacterValidation()
    {
        ErrorMessage = "{0} must have at least 1 special character.";
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessage, name);
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string propertyValue)
        {
            if (!propertyValue.Any(c => char.IsSymbol(c) || char.IsPunctuation(c)))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }
        return ValidationResult.Success;
    }
     
}