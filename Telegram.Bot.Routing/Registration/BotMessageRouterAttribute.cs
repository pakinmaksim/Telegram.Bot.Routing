namespace Telegram.Bot.Routing.Registration;

[AttributeUsage(AttributeTargets.Class)]
public class BotMessageRouterAttribute(string name, bool isDefault = false, string[]? legacyNames = null) : Attribute
{
    public string Name { get; } = name;
    public bool IsDefault { get; } = isDefault;
    public string[] LegacyNames { get; } = legacyNames ?? [];
}