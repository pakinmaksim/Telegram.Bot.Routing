using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Contexts.Messages.RouteResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram.Common;

[MessageRouter("hide_buttons", isDefault: true)]
public class HideButtonsMessageRouter : MessageRouter
{
    [CallbackRoute("any", isDefault: true)]
    public IMessageRouteResult Index()
    {
        return Keyboard(new InlineKeyboardMarkup(Array.Empty<InlineKeyboardButton[]>()));
    }
}