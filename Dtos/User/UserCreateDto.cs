namespace user_management.Dtos.User;

using System.ComponentModel.DataAnnotations;

public class UserCreateDto
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Phone]
    public string? PhoneNumber { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}