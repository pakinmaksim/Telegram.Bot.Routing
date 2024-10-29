using Telegram.Bot.Routing.Contexts.Chats.RouteResults;
using Telegram.Bot.Routing.Contexts.Messages;

namespace Telegram.Bot.Routing.Contexts.Chats;

public abstract class ChatRouter
{
    protected static SendMessageResult Message(MessageStructure message) => 
        new() { Message = message };
    protected static SendRouterMessageResult RouterMessage(string routerName, object? routerData = null) => 
        new() { RouterName = routerName, RouterData = routerData };
    protected static ChatRerouteResult Reroute(string routerName, object? routerData = null) => 
        new() { RouterName = routerName, RouterData = routerData };
    
    protected TelegramChatContext Context { get; private set; } = null!;
    
    
    protected SendRouterMessageResult RouterMessage<TMessageRouter>(object? routerData = null)
        where TMessageRouter : MessageRouter
    {
        var routerName = Context.Routing.GetMessageRouterName(typeof(TMessageRouter));
        return RouterMessage(routerName, routerData);
    }
    protected ChatRerouteResult Reroute<TChatRouter>(object? routerData = null)
        where TChatRouter : ChatRouter
    {
        var routerName = Context.Routing.GetChatRouterName(typeof(TChatRouter));
        return Reroute(routerName, routerData);
    }
}