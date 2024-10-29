namespace Telegram.Bot.Routing.Contexts.Chats.RouteResults;

public class ChatRerouteResult : IChatRouteResult
{
    public string? RouterName { get; set; }
    public object? RouterData { get; set; }
}