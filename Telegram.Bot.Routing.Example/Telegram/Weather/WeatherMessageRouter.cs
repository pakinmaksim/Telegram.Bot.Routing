using Telegram.Bot.Extensions.Markup;
using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Example.Telegram.Greetings;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram.Weather;

[MessageRouter("weather_message")]
public class WeatherMessageRouter : MessageRouter
{
    public async Task Index()
    {
        var data = Context.GetMessageRouterData<WeatherRouterData>();
        if (data is null) Context.SetMessageRouterData(data = new WeatherRouterData());
        
        if (data.Country is null) await AskCountry(data);
        else if (data.City is null) await AskCity(data);
        else await ShowWeatherInfo(data);
    }
    
    private async Task AskCountry(WeatherRouterData data)
    {
        var message = await Context.ShowMessage(new MessageStructure
        {
            Text = "Выберите или введите название страны",
            ReplyMarkup = new InlineKeyboardMarkup([
                [
                    Action("Россия", CallbackData.Create("set_country").SetValue("v", "Россия")),
                    Action("Германия", CallbackData.Create("set_country").SetValue("v", "Германия")),
                    Action("Турция", CallbackData.Create("set_country").SetValue("v", "Турция"))
                ],
                [Action("Отмена", "cancel")]
            ])
        });
        data.LastMessageId = message.TelegramId;
        
        if (!Context.IsChatRouter<WeatherChatRouter>())
            await Context.ChangeChatRouter<WeatherChatRouter>();
        Context.SetChatRouterData(data);
        Context.SetChatRoute("input_country");
    }

    [CallbackRoute("set_country")]
    public async Task SetCountry(CallbackData callback)
    {
        if (!callback.TryGetString("v", out var country)) return;

        var data = Context.UpdateMessageRouterData<WeatherRouterData>(x => x.Country = country)!;
        await AskCity(data);
    }
    
    private async Task AskCity(WeatherRouterData data)
    {
        string[] cityNames = data.Country switch
        {
            "Россия" => ["Москва", "Питер", "Воронеж"],
            "Германия" => ["Бремен", "Кёльн", "Берлин"],
            "Турция" => ["Стамбул", "Анталья", "Бурса"],
            _ => ["Тут", "Там", "Не знаю"]
        };
        var cityButtons = cityNames
            .Select(x => Action(x, CallbackData.Create("set_city").SetValue("v", x)))
            .ToArray();
        var message = await Context.ShowMessage(new MessageStructure
        {
            Text = "Выберите или введите название города",
            ReplyMarkup = new InlineKeyboardMarkup([
                cityButtons,
                [Action("Отмена", "cancel")]
            ])
        });
        data.LastMessageId = message.TelegramId;
        
        if (!Context.IsChatRouter<WeatherChatRouter>())
            await Context.ChangeChatRouter<WeatherChatRouter>();
        Context.SetChatRouterData(data);
        Context.SetChatRoute("input_city");
    }

    [CallbackRoute("set_city")]
    public async Task SetCity(CallbackData callback)
    {
        if (!callback.TryGetString("v", out var city)) return;
        
        var data = Context.UpdateMessageRouterData<WeatherRouterData>(x => x.City = city)!;
        await ShowWeatherInfo(data);
        await Context.SendRouterMessage<GreetingsMessageRouter>();
    }
    
    private async Task ShowWeatherInfo(WeatherRouterData data)
    {
        await Context.ShowMessage("Загрузка\\.\\.\\.");
        await Task.Delay(1500);
        
        var temperatureValue = new Random($"{data.Country}.{data.City}".GetHashCode()).Next(-10, 31);
        var temperature = Tools.EscapeMarkdown(temperatureValue.ToString(), Context.Routing.Config.DefaultParseMode);
        var comment = "Завтра без изменений";
        
        await Context.ShowMessage(new MessageStructure()
        {
            Text = $"*Температура*\\: {temperature} градусов\n" +
                   $"*Комментарий*\\: {comment}\n" +
                   $"*Место*\\: {data.Country}, {data.City}"
        });
    }

    [CallbackRoute("cancel")]
    public async Task Cancel()
    {
        await Context.ChangeMessageRouter<GreetingsMessageRouter>();
    }
}