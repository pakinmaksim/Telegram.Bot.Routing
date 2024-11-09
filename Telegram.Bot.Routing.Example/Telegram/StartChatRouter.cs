using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Example.Telegram.Greetings;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Example.Telegram;

[ChatRouter("start", isDefault: true)]
public class StartChatRouter : ChatRouter
{
    [MessageRoute("any", isDefault: true)]
    public async Task Index(Message message, CancellationToken ct)
    {
        if (Context.Chat.IsPrivate)
            await Context.SendRouterMessage<GreetingsMessageRouter>(ct: ct);
        else
            await Context.RemoveChatRouter(ct);
    }
}