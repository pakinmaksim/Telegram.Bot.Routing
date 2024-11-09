using Telegram.Bot.Routing.Contexts.Chats;

namespace Telegram.Bot.Routing.Example.Telegram.Greetings;

[ChatRouter("greetings")]
public class GreetingsChatRouter : ChatRouter
{
    [MessageRoute("default", isDefault: true)]
    public async Task Default()
    {
        await Context.SendMessage("Выберите пункт меню");
        await Context.SendRouterMessage<GreetingsMessageRouter>();
    }
}