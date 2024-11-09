using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Example.Telegram.Greetings;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Example.Telegram.Weather;

[ChatRouter("weather_chat")]
public class WeatherChatRouter : ChatRouter
{
    [MessageRoute("input_country")]
    public async Task InputCountry(Message message)
    {
        var country = message.Text;
        if (string.IsNullOrEmpty(country))
        {
            await Context.SendMessage("Необходимо любое название страны, попробуйте ещё раз");
            return;
        }

        var data = Context.GetChatRouterData<WeatherRouterData>()!;
        if (data.LastMessageId.HasValue)
        {
            await Context.RemoveKeyboard(data.LastMessageId.Value);
            data.LastMessageId = null;
        }
        data.Country = country;
        Context.SetChatRouterData(data);
        
        await Context.SendRouterMessage<WeatherMessageRouter>(data);
    }
    
    [MessageRoute("input_city")]
    public async Task InputCity(Message message)
    {
        var city = message.Text;
        if (string.IsNullOrEmpty(city))
        {
            await Context.SendMessage("Необходимо любое название города, попробуйте ещё раз");
            return;
        }
        
        var data = Context.GetChatRouterData<WeatherRouterData>()!;
        if (data.LastMessageId.HasValue)
        {
            await Context.RemoveKeyboard(data.LastMessageId.Value);
            data.LastMessageId = null;
        }
        data.City = city;
        Context.SetChatRouterData(data);
        
        await Context.SendRouterMessage<WeatherMessageRouter>(data);
        await Context.SendRouterMessage<GreetingsMessageRouter>();
    }
}