namespace Telegram.Bot.Routing.Contexts.Chats;

[AttributeUsage(AttributeTargets.Method)]
public class MessageRouteAttribute : Attribute
{
    public string Name { get; }
    public bool IsDefault { get; }
    
    public MessageRouteAttribute(string name, bool isDefault = false)
    {
        Name = name;
        IsDefault = isDefault;
    }
}