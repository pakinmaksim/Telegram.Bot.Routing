using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Contexts.Messages.RouteResults;
using Telegram.Bot.Routing.Example.Telegram.Authorization;
using Telegram.Bot.Routing.Example.Telegram.Common;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram.Greetings;

[MessageRouter("greetings")]
public class GreetingsMessageRouter : MessageRouter
{
    [CallbackRoute("index")]
    public IMessageRouteResult Index()
    {
        Context.ButtonPressRequired();
        return Message(new MessageStructure
        {
            Text = "Что делать?",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Регистрация", "reg")],
                [Action("Авторизация", "auth")],
                [Action("О нас", "about")]
            ])
        });
    }
    
    [CallbackRoute("reg")]
    public IMessageRouteResult Registration()
    {
        return Message(new MessageStructure()
        {
            Text = "Регистрации ещё нет",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Назад", "index")],
            ])
        });
    }
    
    [CallbackRoute("auth")]
    public IMessageRouteResult Authorization()
    {
        return Reroute<AuthorizationMessageRouter>();
    }
    
    [CallbackRoute("about")]
    public IMessageRouteResult About()
    {
        return Message(new MessageStructure()
        {
            Text = "Звони сюда \\+79969597212",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Назад", "index")],
            ])
        });
    }
}