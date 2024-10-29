namespace Telegram.Bot.Routing.Contexts.Chats.RouteResults;

public class SendMessageResult : IChatRouteResult
{
    public MessageStructure Message { get; set; } = null!;
}