using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class Login
{
    public string? Username { get; set; } = null;
    [EmailAddress]
    public string? Email { get; set; } = null;
    [Password]
    public string Password { get; set; } = null!;
}