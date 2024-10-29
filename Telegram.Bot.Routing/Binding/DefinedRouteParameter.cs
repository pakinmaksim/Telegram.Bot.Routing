namespace Telegram.Bot.Routing.Binding;

internal class DefinedRouteParameter
{
    public RouteParameterKind Kind { get; init; }
    public Type? Type { get; init; }
    public object? DefaultValue { get; set; }
}