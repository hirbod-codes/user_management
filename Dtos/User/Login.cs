namespace user_management.Dtos.User;

public class Login
{
    public string? Username { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}