using Telegram.Bot.Routing.Contexts.Messages.RouteResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.Messages;

public abstract class MessageRouter
{
    protected static ShowMessageResult Message(MessageStructure message) => 
        new() { Message = message };
    protected static SetKeyboardResult Keyboard(InlineKeyboardMarkup keyboard) => 
        new() { Keyboard = keyboard };
    protected static MessageRerouteResult Reroute(string? routerName, object? routerData = null) => 
        new() { RouterName = routerName, RouterData = routerData };
    protected static ChatRerouteResult RerouteChat(string? routerName, object? routerData = null) => 
        new() { RouterName = routerName, RouterData = routerData };

    protected static InlineKeyboardButton Action(string text, CallbackData callbackData) => 
        InlineKeyboardButton.WithCallbackData(text, callbackData.ToString());

    protected static InlineKeyboardButton Action(string text, string route) => 
        Action(text, CallbackData.Create(route));

    protected TelegramMessageContext Context { get; private set; } = null!;
    
    protected MessageRerouteResult Reroute<TMessageRouter>(object? routerData = null)
        where TMessageRouter : MessageRouter
    {
        var routerName = Context.Routing.GetMessageRouterName(typeof(TMessageRouter));
        return Reroute(routerName, routerData);
    }
    protected ChatRerouteResult RerouteChat<TChatRouter>(object? routerData = null)
        where TChatRouter : MessageRouter
    {
        var routerName = Context.Routing.GetChatRouterName(typeof(TChatRouter));
        return RerouteChat(routerName, routerData);
    }
}