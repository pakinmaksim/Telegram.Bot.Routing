using Telegram.Bot.Routing.Contexts.Chats;

namespace Telegram.Bot.Routing.Example.Telegram.Common;

[ChatRouter("ignore")]
public class IgnoreChatRouter : ChatRouter
{
    [MessageRoute("default", isDefault: true)]
    public void Default() { }
}