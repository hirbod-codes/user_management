using System.Net;
using System.Net.Mail;

namespace user_management.Notification;

public class NotificationProvider: INotificationProvider
{
    public NotificationOptions NotificationOptions { get; }

    public NotificationProvider(NotificationOptions notificationOptions) => NotificationOptions = notificationOptions;

    public async Task Notify(string to, string body, string subject, EmailNotificationOptions? emailNotificationOptions = null)
    {
        emailNotificationOptions ??= new EmailNotificationOptions();

        MailMessage mail = new()
        {
            From = new MailAddress(NotificationOptions.ServerEmailAddress),
            Subject = subject,
            IsBodyHtml = emailNotificationOptions.IsBodyHtml,
            Body = body,
        };
        mail.To.Add(to);

        SmtpClient smtpClient = new()
        {
            Host = NotificationOptions.ServerSmtpHostAddress,
            Port = NotificationOptions.ServerSmtpPort,
            Credentials = new NetworkCredential(NotificationOptions.ServerEmailAddress, NotificationOptions.ServerEmailPassword),
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(mail);
    }

    public Task Notify(string to, string content, PhoneNotificationOptions? phoneNotificationOptions = null)
    {
        throw new NotImplementedException();
    }
}

public interface INotificationProvider
{
    public Task Notify(string to, string body, string subject, EmailNotificationOptions? emailNotificationOptions = null);
    public Task Notify(string to, string content, PhoneNotificationOptions? phoneNotificationOptions = null);
}
