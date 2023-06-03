namespace user_management.Dtos.User;

public class ChangePassword
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PasswordConfirmation { get; set; } = null!;
    public string VerificationSecret { get; set; } = null!;
}