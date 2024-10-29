namespace Telegram.Bot.Routing.Contexts.Chats.RouteResults;

public class SendRouterMessageResult : IChatRouteResult
{
    public string RouterName { get; set; } = null!;
    public object? RouterData { get; set; }
}