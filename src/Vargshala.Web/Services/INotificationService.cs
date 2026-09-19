using Vargshala.Web.Components.UI.Notifications;

namespace Vargshala.Web.Services;

public interface INotificationService
{
    event Action<NotificationItem>? OnNotificationReceived;
    event Action<Guid>? OnNotificationDismissed;

    void Show(AlertType type, string message, string? title = null, int durationSeconds = 2);
    void Success(string message, string? title = "Success", int durationSeconds = 2);
    void Error(string message, string? title = "Error", int durationSeconds = 2);
    void Failed(string message, string? title = "Action Failed", int durationSeconds = 2);
    void Warning(string message, string? title = "Warning", int durationSeconds = 2);
    void Info(string message, string? title = "Information", int durationSeconds = 2);
    void Theme(string message, string? title = "Theme Update", int durationSeconds = 2);
    void Dismiss(Guid id);
}
