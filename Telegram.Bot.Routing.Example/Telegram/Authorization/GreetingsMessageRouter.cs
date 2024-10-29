using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram.Authorization;

[MessageRouter("greetings")]
public class GreetingsMessageRouter : MessageRouter
{
    [CallbackRoute("index")]
    public MessageStructure Index()
    {
        return new MessageStructure()
        {
            Text = "Что делать?",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Регистрация", "reg")],
                [Action("Авторизация", "auth")],
                [Action("О нас", "about")],
            ])
        };
    }
    
    [CallbackRoute("reg")]
    public MessageStructure Registration()
    {
        return new MessageStructure()
        {
            Text = "Регистрации ещё нет",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Назад", "index")],
            ])
        };
    }
    
    [CallbackRoute("auth")]
    public MessageStructure Authorization()
    {
        return new MessageStructure()
        {
            Text = "Регистрации ещё нет 2",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Назад", "index")],
            ])
        };
    }
    
    [CallbackRoute("about")]
    public MessageStructure About()
    {
        return new MessageStructure()
        {
            Text = "Звони сюда \\+79969597212",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Назад", "index")],
            ])
        };
    }
}