namespace user_management.Dtos.User;

public class LoginResult
{
    public string UserId { get; set; } = null!;
    public string Jwt { get; set; } = null!;
}
