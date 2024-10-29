using System.Reflection;

namespace Telegram.Bot.Routing.Binding;

internal class DefinedRoute
{
    public Type RouterType { get; set; } = null!;
    public required MethodInfo Method { get; init; }
    public required DefinedRouteParameter[] Parameters { get; init; }
}