using System.ComponentModel.DataAnnotations;

public class ConfigWithValidation
{
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$")]
    public string? Email { get; set; }
    [Range(0, 1000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public int NumericRange { get; set; }
}