namespace user_management.Dtos.User;

public class UserCreateDto
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = null!;
    public string? Phonenumber { get; set; }
    public string? Username { get; set; }
    public string Password { get; set; } = null!;
    public string? VerificationSecret { get; set; }
}