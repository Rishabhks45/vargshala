namespace Vargshala.Infrastructure.Settings;

public class SendGridOptions
{
    public const string SectionName = "SendGrid";

    public string ApiKey { get; set; } = string.Empty;
    public string DefaultFromEmail { get; set; } = "risham2015sharma@gmail.com";
    public string DefaultFromName { get; set; } = "Vargshala";
}
