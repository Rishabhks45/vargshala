using Vargshala.Web.Components.UI.Notifications;

namespace Vargshala.Web.Services;

public interface INotificationService
{
    event Action<NotificationItem>? OnNotificationReceived;
    event Action<Guid>? OnNotificationDismissed;

    void Show(AlertType type, string message, string? title = null, int durationSeconds = 4);
    void Success(string message, string? title = "Success", int durationSeconds = 4);
    void Error(string message, string? title = "Error", int durationSeconds = 5);
    void Failed(string message, string? title = "Action Failed", int durationSeconds = 5);
    void Warning(string message, string? title = "Warning", int durationSeconds = 5);
    void Info(string message, string? title = "Information", int durationSeconds = 4);
    void Dismiss(Guid id);
}
