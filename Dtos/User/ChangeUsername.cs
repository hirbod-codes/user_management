namespace user_management.Dtos.User;

public class ChangeUsername
{
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string VerificationSecret { get; set; } = null!;
}