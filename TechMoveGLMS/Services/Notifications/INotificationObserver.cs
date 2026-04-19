namespace TechMoveGLMS.Services.Notifications
{
    // Observer Pattern - Observer interface
    public interface INotificationObserver
    {
        Task NotifyAsync(string subject, string message, string recipient);
    }
}