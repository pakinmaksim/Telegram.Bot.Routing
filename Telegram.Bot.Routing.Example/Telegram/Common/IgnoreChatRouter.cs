using Telegram.Bot.Routing.Contexts.Chats;

namespace Telegram.Bot.Routing.Example.Telegram.Common;

[ChatRouter("ignore")]
public class IgnoreChatRouter : ChatRouter
{
    [MessageRoute("any", isDefault: true)]
    public void Index() { }
}