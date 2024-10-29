using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.Messages.RouteResults;

public class SetKeyboardResult : IMessageRouteResult
{
    public InlineKeyboardMarkup Keyboard { get; set; } = null!;
}