namespace user_management.Dtos.User;

public class ChangeEmail
{
    public string Email { get; set; } = null!;
    public string NewEmail { get; set; } = null!;
    public string VerificationSecret { get; set; } = null!;
}