namespace Vargshala.Infrastructure.Settings;

public class ResendOptions
{
    public const string SectionName = "Resend";

    public string ApiKey { get; set; } = string.Empty;
    public string DefaultFromEmail { get; set; } = "onboarding@resend.dev";
    public string DefaultFromName { get; set; } = "Vargshala";
}
