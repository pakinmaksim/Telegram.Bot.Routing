namespace Telegram.Bot.Routing.Contexts.Messages.RouteResults;

public class MessageRerouteResult : IMessageRouteResult
{
    public string? RouterName { get; set; }
    public object? RouterData { get; set; }
}