using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Contexts.Chats.RouteResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram.Authorization;

[ChatRouter("authorization")]
public class AuthorizationChatRouter : ChatRouter
{
    public IChatRouteResult Index()
    {
        return RouterMessage<AuthorizationMessageRouter>();
    }
}