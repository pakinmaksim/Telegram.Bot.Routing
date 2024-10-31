namespace Telegram.Bot.Routing.Contexts.Chats;

/// <summary>
/// Specifies required settings for configuring a ChatRouter, including a unique identifier and default behavior
/// </summary>
/// <param name="name">The unique name to identify the chat router</param>
/// <param name="isDefault">Indicates if this router should be used as the default, including handling the initial state and any unmatched state (set to true if so)</param>
[AttributeUsage(AttributeTargets.Class)]
public class ChatRouterAttribute(string name, bool isDefault = false) : Attribute
{
    public string Name { get; } = name;
    public bool IsDefault { get; } = isDefault;
}