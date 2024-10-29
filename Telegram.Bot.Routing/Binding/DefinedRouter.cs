using System.Reflection;

namespace Telegram.Bot.Routing.Binding;

internal class DefinedRouter
{
    public required string Name { get; init; }
    public required bool IsDefault { get; init; }
    public required TypeInfo Type { get; init; }
    public required DefinedRoute IndexRoute { get; init; }
    public required Dictionary<string, DefinedRoute> Routes { get; init; }
}