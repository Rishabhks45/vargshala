using System.ComponentModel.DataAnnotations;

namespace Vargshala.Contracts.Common;

/// <summary>
/// Universal operational status for entities, records, and dropdown filters across Vargshala SaaS.
/// </summary>
public enum EntityStatus
{
    [Display(Name = "All Statuses")]
    All = 0,

    [Display(Name = "Active")]
    Active = 1,

    [Display(Name = "Inactive")]
    Inactive = 2
}

public static class EntityStatusNames
{
    public const string All = "All";
    public const string Active = "Active";
    public const string Inactive = "Inactive";
}

public static class EntityStatusExtensions
{
    public static string GetDisplayName(this EntityStatus status) => status switch
    {
        EntityStatus.All => "All Statuses",
        EntityStatus.Active => EntityStatusNames.Active,
        EntityStatus.Inactive => EntityStatusNames.Inactive,
        _ => status.ToString()
    };

    public static bool? ToNullableBool(this EntityStatus status) => status switch
    {
        EntityStatus.Active => true,
        EntityStatus.Inactive => false,
        _ => null
    };

    public static EntityStatus FromBool(bool isActive) => isActive ? EntityStatus.Active : EntityStatus.Inactive;

    public static string ToStatusName(this bool isActive) => isActive ? EntityStatusNames.Active : EntityStatusNames.Inactive;

    public static string ToStatusName(this bool? isActive) => isActive switch
    {
        true => EntityStatusNames.Active,
        false => EntityStatusNames.Inactive,
        _ => EntityStatusNames.All
    };

    public static EntityStatus FromNullableBool(bool? isActive) => isActive switch
    {
        true => EntityStatus.Active,
        false => EntityStatus.Inactive,
        null => EntityStatus.All
    };

    public static EntityStatus FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return EntityStatus.All;
        var trimmed = value.Trim();
        if (bool.TryParse(trimmed, out var b)) return b ? EntityStatus.Active : EntityStatus.Inactive;
        if (trimmed.Equals("Suspended", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("Disabled", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
        {
            return EntityStatus.Inactive;
        }
        if (trimmed.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
        {
            return EntityStatus.Active;
        }
        if (Enum.TryParse<EntityStatus>(trimmed, true, out var result)) return result;
        return EntityStatus.All;
    }
}
