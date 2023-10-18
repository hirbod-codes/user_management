namespace user_management.Notification;

public class NotificationOptions
{
    public string ServerEmailAddress { get; set; } = null!;
    public string ServerSmtpHostAddress { get; set; } = null!;
    public string ServerEmailPassword { get; set; } = null!;
    public int ServerSmtpPort { get; set; }
}
