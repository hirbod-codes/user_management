namespace user_management.Utilities;

using System.Net;
using System.Net.Mail;

public class NotificationHelper : INotificationHelper
{
    public void SendVerificationMessage(string email, string verificationCode)
    {
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress("taghalloby@gmail.com");
        mail.To.Add(email);
        mail.Subject = "Verificaiton email from user_management";
        mail.Body = $@"Dear user,
here's your verification code: {verificationCode}

Regards <b>user_management</b>";
        SmtpClient smtpClient = new SmtpClient();
        smtpClient.Host = "smtp.gmail.com";
        smtpClient.Port = 587;
        smtpClient.Credentials = new NetworkCredential("taghalloby@gmail.com", "ihtz hgea bxnt gqya");
        smtpClient.EnableSsl = true;
        smtpClient.SendAsync(mail, null);
    }

}