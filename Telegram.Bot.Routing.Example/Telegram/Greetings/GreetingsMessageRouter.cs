using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Example.Telegram.Weather;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram.Greetings;

[MessageRouter("greetings")]
public class GreetingsMessageRouter : MessageRouter
{
    [CallbackRoute("index", isDefault: true)]
    public async Task Index()
    {
        if (!Context.IsChatRouter<GreetingsChatRouter>()) 
            await Context.ChangeChatRouter<GreetingsChatRouter>();
        
        await Context.ShowMessage(new MessageStructure
        {
            Text = "Добро пожаловать в бот погоды",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Узнать погоду", "location")],
                [Action("О нас", "about")]
            ])
        });
    }
    
    [CallbackRoute("location")]
    public async Task Location()
    {
        await Context.ChangeMessageRouter<WeatherMessageRouter>();
    }
    
    [CallbackRoute("about")]
    public async Task About()
    {
        await Context.ShowMessage(new MessageStructure()
        {
            Text = "Просто образец бота, используйте исходный код для примера",
            ReplyMarkup = new InlineKeyboardMarkup([
                [Action("Назад", "index")],
            ])
        });
    }
}