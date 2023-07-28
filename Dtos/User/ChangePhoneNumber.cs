namespace user_management.Dtos.User;

public class ChangePhoneNumber
{
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string VerificationSecret { get; set; } = null!;
}