using System.ComponentModel.DataAnnotations;

namespace user_management.Dtos.User;

public class ChangeUsername
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    [MinLength(6)]
    public string VerificationSecret { get; set; } = null!;
}
