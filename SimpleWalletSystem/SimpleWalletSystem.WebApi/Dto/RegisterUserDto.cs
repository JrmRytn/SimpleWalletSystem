namespace SimpleWalletSystem.WebApi.Dto;

public class RegisterUserDto  
{ 
    [Required] 
    [StringLength(12, MinimumLength = 6, ErrorMessage = "Login name must be between 6 and 12 characters.")]
    [RequiredLetterOrDigitOnlyValidation]
    public string LoginName { get; set; }
    [Required, MinLength(8,ErrorMessage = "Password must be at least 8 characters.")]
    [RequiredUpperValidation]
    [RequiredDigitValidation]
    [RequiredSpecialCharacterValidation]
    public string Password { get; set; }
     
}
