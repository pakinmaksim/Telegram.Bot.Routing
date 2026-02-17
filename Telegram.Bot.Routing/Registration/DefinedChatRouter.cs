namespace Telegram.Bot.Routing.Registration;

public class DefinedChatRouter
{
    public required string Name { get; init; }
    public string[] LegacyNames { get; set; } = [];
    public required Type Type { get; init; }
    public Type? DataType { get; init; }
    public bool IsDefault { get; set; } = false;

    public IEnumerable<string> Names
    {
        get
        {
            yield return Name;
            foreach (var legacyName in LegacyNames)
                yield return legacyName;
        }
    }
}