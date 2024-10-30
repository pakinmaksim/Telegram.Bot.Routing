using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Contexts.Messages.RouteResults;
using Telegram.Bot.Routing.Example.Telegram.Greetings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram.Authorization;

[MessageRouter("authorization")]
public class AuthorizationMessageRouter : MessageRouter
{
    [CallbackRoute("index")]
    public IMessageRouteResult Index(CallbackQuery callbackQuery)
    {
        return Message(new MessageStructure()
        {
            Text = "Номер ВУ введи",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Отмена", "cancel")]
            ])
        });
    }
    
    [CallbackRoute("cancel")]
    public IMessageRouteResult ReturnToGreetings()
    {
        return Reroute<GreetingsMessageRouter>();
    }
}