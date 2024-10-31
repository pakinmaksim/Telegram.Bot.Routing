namespace Telegram.Bot.Routing.Contexts.Chats;

/// <summary>
/// Specifies required settings for configuring a message route within a ChatRouter, including a unique identifier and default behavior.
/// Use this attribute to define how the chat should react to a message when the current route is active.
/// </summary>
/// <param name="name">The unique name to identify the message route within the current ChatRouter</param>
/// <param name="isDefault">Indicates if this route should be used as the default for any unmatched state (set to true if so)</param>
[AttributeUsage(AttributeTargets.Method)]
public class MessageRouteAttribute(string name, bool isDefault = false) : Attribute
{
    public string Name { get; } = name;
    public bool IsDefault { get; } = isDefault;
}