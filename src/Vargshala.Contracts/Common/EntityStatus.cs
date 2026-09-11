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

    public static EntityStatus FromNullableBool(bool? isActive) => isActive switch
    {
        true => EntityStatus.Active,
        false => EntityStatus.Inactive,
        null => EntityStatus.All
    };

    public static EntityStatus FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return EntityStatus.All;
        if (bool.TryParse(value, out var b)) return b ? EntityStatus.Active : EntityStatus.Inactive;
        if (Enum.TryParse<EntityStatus>(value, true, out var result)) return result;
        return EntityStatus.All;
    }
}
