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
