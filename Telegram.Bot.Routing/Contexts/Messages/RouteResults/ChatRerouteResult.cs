namespace Telegram.Bot.Routing.Contexts.Messages.RouteResults;

public class ChatRerouteResult : IMessageRouteResult
{
    public string? RouterName { get; set; }
    public object? RouterData { get; set; }
}