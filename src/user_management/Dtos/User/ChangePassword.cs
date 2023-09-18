using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

[PasswordConfirmationAttribute<ChangePassword>]
public class ChangePassword : IPasswordConfirmable
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Password]
    public string Password { get; set; } = null!;
    [Password]
    public string PasswordConfirmation { get; set; } = null!;
    [MinLength(6)]
    public string VerificationSecret { get; set; } = null!;
}