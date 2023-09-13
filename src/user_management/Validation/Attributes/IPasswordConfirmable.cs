namespace user_management.Validation.Attributes;

public interface IPasswordConfirmable
{
    public string Password { get; set; }
    public string PasswordConfirmation { get; set; }
}
