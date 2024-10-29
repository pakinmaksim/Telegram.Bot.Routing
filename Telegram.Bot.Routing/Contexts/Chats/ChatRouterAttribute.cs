namespace Telegram.Bot.Routing.Contexts.Chats;

[AttributeUsage(AttributeTargets.Class)]
public class ChatRouterAttribute(string name, bool isDefault = false) : Attribute
{
    public string Name { get; } = name;
    public bool IsDefault { get; } = isDefault;
}