using System.ComponentModel.DataAnnotations;

namespace user_management.Dtos.User;

public class ChangeEmail
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    [EmailAddress]
    public string NewEmail { get; set; } = null!;
    [MinLength(6)]
    [MaxLength(6)]
    public string VerificationSecret { get; set; } = null!;
}