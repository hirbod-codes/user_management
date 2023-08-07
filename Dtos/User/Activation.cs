using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class Activation
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Password]
    public string Password { get; set; } = null!;
    [MinLength(6)]
    [MaxLength(6)]
    public string VerificationSecret { get; set; } = null!;
}