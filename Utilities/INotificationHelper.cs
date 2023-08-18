using System.Net.Mail;

namespace user_management.Utilities;

public interface INotificationHelper
{
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="SmtpException"></exception>
    public Task SendVerificationMessage(string email, string verificationCode);
}