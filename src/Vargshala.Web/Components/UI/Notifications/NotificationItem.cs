namespace Vargshala.Web.Components.UI.Notifications;

public class NotificationItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Title { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlertType Type { get; set; } = AlertType.Info;
    public int DurationSeconds { get; set; } = 4;
    public bool Dismissible { get; set; } = true;
}
