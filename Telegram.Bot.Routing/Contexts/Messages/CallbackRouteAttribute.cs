namespace Telegram.Bot.Routing.Contexts.Messages;

/// <summary>
/// Specifies the required settings for configuring a callback route within a MessageRouter, including a unique identifier and default behavior.
/// Use this attribute to define how the message should react to a callback query triggered by pressing a button on this message.
/// </summary>
/// <param name="name">The unique name to identify the message route within the current ChatRouter</param>
/// <param name="isDefault">Indicates if this route should be used as the default for any unmatched state (set to true if so)</param>
[AttributeUsage(AttributeTargets.Method)]
public class CallbackRouteAttribute(string name, bool isDefault = false) : Attribute
{
    public string Name { get; } = name;
    public bool IsDefault { get; } = isDefault;
}