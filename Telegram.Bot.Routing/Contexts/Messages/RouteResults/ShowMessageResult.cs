namespace Telegram.Bot.Routing.Contexts.Messages.RouteResults;

public class ShowMessageResult : IMessageRouteResult
{
    public MessageStructure Message { get; set; } = null!;
}