namespace Vargshala.Web.Common;

/// <summary>
/// Represents an emoji category grouping for emoji pickers.
/// </summary>
public class EmojiCategory
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<string> Emojis { get; set; } = new();
}

/// <summary>
/// Helper providing static emoji catalogs, quick reactions, keywords, and search filtering.
/// </summary>
public static class EmojiHelper
{
    /// <summary>
    /// Default quick reaction emojis shown on hover over chat messages.
    /// </summary>
    public static readonly IReadOnlyList<string> QuickReactions = new[]
    {
        "👍", "❤️", "😂", "😮", "😢", "🙏", "🔥", "🎉"
    };

    /// <summary>
    /// Full emoji categories for the categorized emoji picker.
    /// </summary>
    public static readonly IReadOnlyList<EmojiCategory> Categories = new List<EmojiCategory>
    {
        new EmojiCategory
        {
            Key = "Smileys",
            Title = "Smileys & Emotion",
            Icon = "😀",
            Emojis = new() { "😀", "😃", "😄", "😁", "😆", "😅", "😂", "🤣", "😊", "😇", "🙂", "🙃", "😉", "😌", "😍", "🥰", "😘", "😗", "😙", "😚", "😋", "😛", "😝", "😜", "🤪", "🤨", "🧐", "🤓", "😎", "🤩", "🥳", "😏", "😒", "😞", "😔", "😟", "😕", "🙁", "☹️", "😣", "😖", "😫", "😩", "🥺", "😢", "😭", "😤", "😠", "😡", "🤬", "🤯", "😳", "🥵", "🥶", "😱", "😨", "😰", "😥", "😓", "🤗", "🤔", "🤭", "🤫", "🤥", "😶", "😐", "😑", "😬", "🙄", "😯", "😦", "😧", "😮", "😲", "🥱", "😴", "🤤", "😪", "😵", "🤐", "🥴", "🤢", "🤮", "🤧", "😷", "🤒", "🤕" }
        },
        new EmojiCategory
        {
            Key = "Gestures",
            Title = "People & Hands",
            Icon = "👋",
            Emojis = new() { "👍", "👎", "👌", "🤌", "🤏", "✌️", "🤞", "🫰", "🤟", "🤘", "🤙", "👈", "👉", "👆", "🖕", "👇", "☝️", "✋", "🤚", "🖐️", "🖖", "👋", "🤝", "👏", "🙌", "👐", "🤲", "🙏", "✍️", "💅", "🤳", "💪", "🦾" }
        },
        new EmojiCategory
        {
            Key = "Hearts",
            Title = "Hearts & Symbols",
            Icon = "❤️",
            Emojis = new() { "❤️", "🧡", "💛", "💚", "💙", "💜", "🖤", "🤍", "🤎", "💔", "❤️‍🔥", "❤️‍🩹", "❣️", "💕", "💞", "💓", "💗", "💖", "💘", "💝", "💟", "💯", "💢", "💥", "💫", "💦", "💨", "✨", "🌟", "⭐" }
        },
        new EmojiCategory
        {
            Key = "Party",
            Title = "Activities & Objects",
            Icon = "🎉",
            Emojis = new() { "🔥", "🎉", "🎊", "🎈", "🎁", "🏆", "🥇", "🥈", "🥉", "🏅", "🎖️", "🎯", "🚀", "💡", "📚", "📖", "✏️", "✒️", "📝", "📌", "📍", "📎", "🔒", "🔓", "🔑", "🔔", "🔕", "📢", "📣", "🔍", "⏳", "⏰", "📅" }
        },
        new EmojiCategory
        {
            Key = "Food",
            Title = "Food & Nature",
            Icon = "☕",
            Emojis = new() { "☕", "🍵", "🧃", "🥤", "🍎", "🍉", "🍇", "🍓", "🍒", "🍑", "🥭", "🍍", "🥥", "🥝", "🍕", "🍔", "🍟", "🌭", "🥪", "🌮", "🌯", "🍿", "🍣", "🍦", "🍩", "🍪", "🎂", "🍫", "☀️", "🌙", "☁️", "🌧️", "⚡", "🌈", "🌸", "🌹", "🌻" }
        }
    };

    /// <summary>
    /// Search keywords dictionary for emoji matching.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string[]> Keywords = new Dictionary<string, string[]>
    {
        ["😀"] = new[] { "smile", "happy", "grin", "face" },
        ["😃"] = new[] { "smile", "happy", "joy", "face" },
        ["😄"] = new[] { "laugh", "happy", "joy", "face" },
        ["😁"] = new[] { "grin", "happy", "teeth", "face" },
        ["😆"] = new[] { "laugh", "happy", "face" },
        ["😅"] = new[] { "sweat", "laugh", "nervous" },
        ["😂"] = new[] { "joy", "tears", "laugh", "lol" },
        ["🤣"] = new[] { "rofl", "lol", "laugh" },
        ["😊"] = new[] { "blush", "smile", "happy" },
        ["😇"] = new[] { "angel", "innocent", "halo" },
        ["😍"] = new[] { "love", "heart", "eyes" },
        ["🥰"] = new[] { "love", "hearts", "adore" },
        ["😘"] = new[] { "kiss", "love" },
        ["😎"] = new[] { "cool", "sunglasses" },
        ["🤩"] = new[] { "star", "eyes", "wow" },
        ["🥳"] = new[] { "party", "celebrate", "birthday" },
        ["😢"] = new[] { "cry", "sad", "tear" },
        ["😭"] = new[] { "sob", "cry", "sad" },
        ["😡"] = new[] { "angry", "mad", "rage" },
        ["🤯"] = new[] { "mindblown", "shock", "boom" },
        ["👍"] = new[] { "thumbs", "up", "yes", "like", "agree" },
        ["👎"] = new[] { "thumbs", "down", "no", "dislike" },
        ["👌"] = new[] { "ok", "perfect", "good" },
        ["✌️"] = new[] { "peace", "victory", "two" },
        ["🤞"] = new[] { "fingers", "crossed", "luck" },
        ["🤝"] = new[] { "handshake", "deal", "agree" },
        ["👏"] = new[] { "clap", "applause", "bravo" },
        ["🙌"] = new[] { "hooray", "hands", "praise" },
        ["🙏"] = new[] { "pray", "thanks", "please", "namaste" },
        ["💪"] = new[] { "muscle", "flex", "strong", "power" },
        ["❤️"] = new[] { "heart", "love", "red" },
        ["🔥"] = new[] { "fire", "hot", "lit", "trend" },
        ["🎉"] = new[] { "party", "tada", "celebration", "congrats" },
        ["✨"] = new[] { "sparkles", "magic", "clean", "star" },
        ["💯"] = new[] { "100", "score", "perfect" },
        ["🚀"] = new[] { "rocket", "launch", "fast", "moon" },
        ["💡"] = new[] { "bulb", "idea", "smart" },
        ["📚"] = new[] { "books", "study", "education", "read" },
        ["☕"] = new[] { "coffee", "tea", "drink" },
        ["🍕"] = new[] { "pizza", "food", "cheese" },
        ["🎂"] = new[] { "cake", "birthday", "party" }
    };

    /// <summary>
    /// Filters emojis based on search term (keywords) or category key.
    /// </summary>
    public static IEnumerable<string> FilterEmojis(string? activeCategoryKey, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            var matches = new List<string>();
            foreach (var cat in Categories)
            {
                foreach (var em in cat.Emojis)
                {
                    if (Keywords.TryGetValue(em, out var kws) && kws.Any(k => k.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    {
                        matches.Add(em);
                    }
                }
            }

            if (matches.Count > 0)
            {
                return matches.Distinct();
            }

            return Categories.SelectMany(c => c.Emojis).Distinct();
        }

        var active = Categories.FirstOrDefault(c => c.Key == activeCategoryKey);
        return active?.Emojis ?? Categories[0].Emojis;
    }
}
