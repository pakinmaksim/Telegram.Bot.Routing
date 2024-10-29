using System.Reflection;

namespace Telegram.Bot.Routing.Binding;

internal class DefinedMessageRouter
{
    public required string Name { get; init; }
    public required TypeInfo Type { get; init; }
    public required DefinedRoute IndexRoute { get; init; }
    public required DefinedRoute? DefaultCallbackRoute { get; init; }
    public required Dictionary<string, DefinedRoute> CallbackRoutes { get; init; }
}