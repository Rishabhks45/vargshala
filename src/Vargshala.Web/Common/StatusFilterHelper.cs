using Vargshala.Contracts.Common;
using Vargshala.Web.Components.UI.Inputs;

namespace Vargshala.Web.Common;

/// <summary>
/// Universal UI helper for building standardized CustomSelect options from EntityStatus.
/// Used across all data tables and filter bars in Vargshala SaaS.
/// </summary>
public static class StatusFilterHelper
{
    /// <summary>
    /// Returns standardized CustomSelect options for status filtering.
    /// Values correspond to EntityStatus ("Active", "Inactive") and empty string for "All".
    /// Fully parsed by EntityStatusExtensions.FromString(val).ToNullableBool().
    /// </summary>
    public static List<CustomSelect.CustomSelectOption> GetStatusFilterOptions(
        string allLabel = "All Status",
        string activeLabel = "Active Only",
        string inactiveLabel = "Inactive Only")
    {
        return new List<CustomSelect.CustomSelectOption>
        {
            new("", allLabel),
            new(EntityStatusNames.Active, activeLabel),
            new(EntityStatusNames.Inactive, inactiveLabel)
        };
    }
}
