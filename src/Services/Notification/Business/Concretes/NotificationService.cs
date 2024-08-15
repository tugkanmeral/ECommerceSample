public class NotificationService : INotificationService
{
    public void Notify(Notification notification)
    {
        string msg = $"Recipient: {notification.Recipient}, Subject: {notification.Subject}, Message: {notification.Message}";
        Console.WriteLine(msg);
    }
}