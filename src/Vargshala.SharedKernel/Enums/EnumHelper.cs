using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Vargshala.SharedKernel.Enums;

/// <summary>
/// High-performance helper and cache for enum metadata, display names, and dropdown items.
/// Eliminates boilerplate switch statements and duplicated string constants across the solution.
/// </summary>
public static class EnumHelper
{
    private static readonly ConcurrentDictionary<Enum, string> DisplayNameCache = new();
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<EnumItem>> EnumItemsCache = new();

    /// <summary>
    /// Represents an enum item with its member name, display name, and integer value.
    /// Ideal for binding directly to dropdowns, select lists, and API contracts.
    /// </summary>
    public sealed record EnumItem(string Value, string DisplayName, int IntValue);

    /// <summary>
    /// Gets the human-readable display name for any enum value.
    /// Priority:
    /// 1. [Display(Name = "...")]
    /// 2. [Description("...")]
    /// 3. Enum member name (ToString())
    /// Results are cached in a thread-safe ConcurrentDictionary for ultra-fast O(1) lookups.
    /// </summary>
    public static string GetDisplayName(Enum? value)
    {
        if (value == null) return string.Empty;

        return DisplayNameCache.GetOrAdd(value, val =>
        {
            var fieldInfo = val.GetType().GetField(val.ToString());
            if (fieldInfo == null) return val.ToString();

            var displayAttr = fieldInfo.GetCustomAttribute<DisplayAttribute>();
            if (!string.IsNullOrEmpty(displayAttr?.Name))
                return displayAttr.Name;

            var descAttr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            if (!string.IsNullOrEmpty(descAttr?.Description))
                return descAttr.Description;

            return val.ToString();
        });
    }

    /// <summary>
    /// Gets all items of an enum with their name, display name, and integer value.
    /// Cached per enum type for high performance.
    /// </summary>
    public static IReadOnlyList<EnumItem> GetItems<TEnum>() where TEnum : struct, Enum
    {
        return EnumItemsCache.GetOrAdd(typeof(TEnum), _ =>
        {
            var values = Enum.GetValues<TEnum>();
            var list = new List<EnumItem>(values.Length);

            foreach (var val in values)
            {
                list.Add(new EnumItem(
                    Value: val.ToString(),
                    DisplayName: GetDisplayName(val),
                    IntValue: Convert.ToInt32(val)
                ));
            }

            return list.AsReadOnly();
        });
    }

    /// <summary>
    /// Gets all values of an enum type as a typed list.
    /// </summary>
    public static IReadOnlyList<TEnum> GetValues<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>();
    }

    /// <summary>
    /// Gets all display names for the specified enum type.
    /// </summary>
    public static IReadOnlyList<string> GetDisplayNames<TEnum>() where TEnum : struct, Enum
    {
        return GetItems<TEnum>().Select(x => x.DisplayName).ToList();
    }

    /// <summary>
    /// Attempts to parse an enum value from its display name, member name, or integer string.
    /// Case-insensitive. Returns true if match found, false otherwise.
    /// </summary>
    public static bool TryParseFromDisplayName<TEnum>(string? text, out TEnum result) where TEnum : struct, Enum
    {
        result = default;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var trimmed = text.Trim();

        // 1. Direct standard enum parse (matches member name or int value)
        if (Enum.TryParse<TEnum>(trimmed, ignoreCase: true, out result))
            return true;

        // 2. Scan display names cached for this enum
        var items = GetItems<TEnum>();
        foreach (var item in items)
        {
            if (string.Equals(item.DisplayName, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<TEnum>(item.Value, ignoreCase: true, out result))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Parses an enum value from its display name or member name.
    /// Throws ArgumentException if no match is found.
    /// </summary>
    public static TEnum ParseFromDisplayName<TEnum>(string? text) where TEnum : struct, Enum
    {
        if (TryParseFromDisplayName<TEnum>(text, out var result))
            return result;

        throw new ArgumentException($"Requested value or display name '{text}' was not found in enum '{typeof(TEnum).Name}'.");
    }

    /// <summary>
    /// Clears internal reflection caches. Useful for test isolation if needed.
    /// </summary>
    public static void ClearCache()
    {
        DisplayNameCache.Clear();
        EnumItemsCache.Clear();
    }
}

/// <summary>
/// Convenient extension methods on System.Enum for display name resolution.
/// Automatically provides .GetDisplayName() on any enum without boilerplate extension classes.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Returns the cached human-friendly display name of this enum value.
    /// Falls back to enum member name if no [Display] or [Description] attribute exists.
    /// </summary>
    public static string GetDisplayName(this Enum? value) => EnumHelper.GetDisplayName(value);

    /// <summary>
    /// Gets the display name for a nullable enum value, returning empty string if null.
    /// </summary>
    public static string ToDisplayName(this Enum? value) => EnumHelper.GetDisplayName(value);
}
