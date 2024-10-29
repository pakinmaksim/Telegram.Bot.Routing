namespace Telegram.Bot.Routing.Contexts.Messages;

[AttributeUsage(AttributeTargets.Method)]
public class CallbackRouteAttribute : Attribute
{
    public string Name { get; }
    public bool IsDefault { get; }
    
    public CallbackRouteAttribute(string name, bool isDefault = false)
    {
        Name = name;
        IsDefault = isDefault;
    }
}