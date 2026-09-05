namespace Vargshala.Application.Abstractions.Email;

public class EmailMessageRequest
{
    public List<string> To { get; set; } = new();
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public string? From { get; set; }
    public string? ReplyTo { get; set; }
}
