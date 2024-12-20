using System.Reflection;

namespace Telegram.Bot.Routing.Binding;

internal class DefinedChatRouter
{
    public required string Name { get; init; }
    public required TypeInfo Type { get; init; }
    public required DefinedRoute? IndexRoute { get; init; }
    public required DefinedRoute? DefaultMessageRoute { get; init; }
    public required Dictionary<string, DefinedRoute> MessageRoutes { get; init; }
    
    public DefinedRoute? GetMessageRoute(string? routeName)
    {
        if (routeName is null) return DefaultMessageRoute;
        return MessageRoutes.TryGetValue(routeName, out var route) ? route : DefaultMessageRoute;
    }
}