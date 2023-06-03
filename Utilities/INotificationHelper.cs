namespace user_management.Utilities;

public interface INotificationHelper
{
    public void SendVerificationMessage(string email, string verificationCode);
}