namespace Vargshala.Application.Settings;

public class EncryptionSettings
{
    public const string SectionName = "Encryption";
    public string MasterKey { get; set; } = string.Empty;
}
