namespace Vargshala.Infrastructure.Settings;

public class SupabaseStorageOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = "https://ywjkdgzahqfcmetyqdsh.supabase.co";
    public string ApiKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = "Vargshala";
    public string BaseFolder { get; set; } = "Messages_Docs";
}
