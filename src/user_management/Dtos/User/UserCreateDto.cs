namespace user_management.Dtos.User;

using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;

public class UserCreateDto
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress]
    public string Email { get; set; } = null!;
    [RegEx("^[0-9]{11}$", "The {name} format is invalid.")]
    public string? PhoneNumber { get; set; }
    public string Username { get; set; } = null!;
    [Password]
    public string Password { get; set; } = null!;
}