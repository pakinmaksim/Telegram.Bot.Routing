namespace Telegram.Bot.Routing.Contexts.Messages;

[AttributeUsage(AttributeTargets.Class)]
public class MessageRouterAttribute(string name, bool isDefault = false) : Attribute
{
    public string Name { get; } = name;
    public bool IsDefault { get; } = isDefault;
}