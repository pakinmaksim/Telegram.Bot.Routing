using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Contexts.Chats.RouteResults;
using Telegram.Bot.Routing.Example.Telegram.Common;
using Telegram.Bot.Routing.Example.Telegram.Greetings;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Example.Telegram;

[ChatRouter("start", isDefault: true)]
public class StartChatRouter : ChatRouter
{
    [MessageRoute("any", isDefault: true)]
    public IChatRouteResult Index(Message message, CancellationToken ct)
    {
        if (Context.Chat.IsPrivate)
            return RouterMessage<GreetingsMessageRouter>();

        return Reroute<IgnoreChatRouter>();
    }
}