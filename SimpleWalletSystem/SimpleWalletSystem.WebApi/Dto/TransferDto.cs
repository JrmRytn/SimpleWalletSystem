namespace SimpleWalletSystem.WebApi.Dto;

public class TransferDto 
{
    [Required]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Source account number must be 12 digits.")]
    public long AccountSource { get; set; }

    [Required]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Destination account number must be 12 digits.")]
    public long AccountDestination { get; set; } 

}

 
