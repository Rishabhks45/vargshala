using Vargshala.Web.Components.UI.Notifications;

namespace Vargshala.Web.Services;

public class NotificationService : INotificationService
{
    public event Action<NotificationItem>? OnNotificationReceived;
    public event Action<Guid>? OnNotificationDismissed;

    public void Show(AlertType type, string message, string? title = null, int durationSeconds = 4)
    {
        var notification = new NotificationItem
        {
            Type = type,
            Message = message,
            Title = title,
            DurationSeconds = durationSeconds
        };

        OnNotificationReceived?.Invoke(notification);
    }

    public void Success(string message, string? title = "Success", int durationSeconds = 4)
    {
        Show(AlertType.Success, message, title, durationSeconds);
    }

    public void Error(string message, string? title = "Error", int durationSeconds = 5)
    {
        Show(AlertType.Error, message, title, durationSeconds);
    }

    public void Failed(string message, string? title = "Action Failed", int durationSeconds = 5)
    {
        Show(AlertType.Failed, message, title, durationSeconds);
    }

    public void Warning(string message, string? title = "Warning", int durationSeconds = 5)
    {
        Show(AlertType.Warning, message, title, durationSeconds);
    }

    public void Info(string message, string? title = "Information", int durationSeconds = 4)
    {
        Show(AlertType.Info, message, title, durationSeconds);
    }

    public void Dismiss(Guid id)
    {
        OnNotificationDismissed?.Invoke(id);
    }
}
