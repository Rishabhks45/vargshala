namespace Vargshala.Web.Common;

public record ThemeTemplate(
    string Id,
    string Name,
    string Category,
    string HeaderHex,
    string PrimaryHex,
    string GradientEndHex,
    string LightBgHex);

public static class ThemeConstants
{
    public static readonly List<ThemeTemplate> AllThemes = new()
    {
        new("deep-teal", "Deep Teal", "Theme 4 (Default)", "#004D40", "#009488", "#00796B", "#F0FDFA"),
        new("purple", "Purple", "Material Palette", "#4A148C", "#9C27B0", "#6A1B9A", "#F3E5F5"),
        new("deep-purple", "Deep Purple", "Material Palette", "#311B92", "#673AB7", "#4527A0", "#EDE7F6"),
        new("indigo", "Indigo", "Material Palette", "#1A237E", "#3F51B5", "#283593", "#E8EAF6"),
        new("blue", "Blue", "Material Palette", "#0D47A1", "#2196F3", "#1565C0", "#E3F2FD"),
        new("light-blue", "Light Blue", "Material Palette", "#01579B", "#03A9F4", "#0277BD", "#E1F5FE"),
        new("cyan", "Cyan", "Material Palette", "#006064", "#00BCD4", "#00838F", "#E0F7FA"),
        new("teal", "Teal", "Material Palette", "#004D40", "#009688", "#00695C", "#E0F2F1"),
        new("green", "Green", "Material Palette", "#1B5E20", "#4CAF50", "#2E7D32", "#E8F5E9"),
        new("light-green", "Light Green", "Material Palette", "#33691E", "#8BC34A", "#558B2F", "#F1F8E9"),
        new("lime", "Lime", "Material Palette", "#827717", "#CDDC39", "#9E9D24", "#F9FBE7"),
        new("yellow", "Yellow", "Material Palette", "#F57F17", "#FFEB3B", "#F9A825", "#FFFDE7"),
        new("amber", "Amber", "Material Palette", "#FF6F00", "#FFC107", "#FFA000", "#FFF8E1"),
        new("orange", "Orange", "Material Palette", "#E65100", "#FF9800", "#EF6C00", "#FFF3E0"),
        new("deep-orange", "Deep Orange", "Material Palette", "#BF360C", "#FF5722", "#D84315", "#FBE9E7"),
        new("red", "Red", "Material Palette", "#B71C1C", "#F44336", "#C62828", "#FFEBEE"),
        new("pink", "Pink", "Material Palette", "#880E4F", "#E91E63", "#AD1457", "#FCE4EC"),
        new("brown", "Brown", "Material Palette", "#3E2723", "#795548", "#4E342E", "#EFEBE9"),
        new("blue-gray", "Blue Gray", "Material Palette", "#263238", "#607D8B", "#37474F", "#ECEFF1"),
        new("gray", "Gray", "Material Palette", "#212121", "#9E9E9E", "#424242", "#FAFAFA")
    };
}
