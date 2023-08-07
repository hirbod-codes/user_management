using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class ChangePhoneNumber
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    [RegEx("^[0-9]{11}$", "The {name} format is invalid.")]
    public string PhoneNumber { get; set; } = null!;
    [MinLength(6)]
    [MaxLength(6)]
    public string VerificationSecret { get; set; } = null!;
}