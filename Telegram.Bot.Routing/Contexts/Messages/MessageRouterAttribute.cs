namespace Telegram.Bot.Routing.Contexts.Messages;
/// <summary>
/// Specifies required settings for configuring a MessageRouter, including a unique identifier and default behavior
/// </summary>
/// <param name="name">The unique name to identify the message router</param>
/// <param name="isDefault">Indicates if this router should be used as the default for any unmatched state (set to true if so)</param>
[AttributeUsage(AttributeTargets.Class)]
public class MessageRouterAttribute(string name, bool isDefault = false) : Attribute
{
    public string Name { get; } = name;
    public bool IsDefault { get; } = isDefault;
}