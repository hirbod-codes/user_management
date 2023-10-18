namespace user_management.Utilities;

using user_management.Notification;

public class NotificationHelper : INotificationHelper
{
    private readonly INotificationProvider NotificationProvider;

    public NotificationHelper(INotificationProvider notificationProvider) => NotificationProvider = notificationProvider;

    public async Task SendVerificationMessage(string email, string verificationCode) => await NotificationProvider.Notify(email, $@"Dear user
here's your verification code: {verificationCode}

Regards <b>user_management</b>", "Verification email from user management", new() { IsBodyHtml = true });
}

// MailMessage mail = new()
// {
//     From = new MailAddress("taghalloby@gmail.com"),
//     Subject = "Verification email from user_management",
//     IsBodyHtml = true,
//     Body = $@"",
// };
// mail.To.Add(email);

// SmtpClient smtpClient = new()
// {
//     Host = "smtp.gmail.com",
//     Port = 587,
//     Credentials = new NetworkCredential("taghalloby@gmail.com", "ihtz hgea bxnt gqya"),
//     EnableSsl = true
// };
// await smtpClient.SendMailAsync(mail);
