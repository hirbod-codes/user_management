namespace user_management.Dtos.User;

public class Activation
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string VerificationSecret { get; set; } = null!;
}