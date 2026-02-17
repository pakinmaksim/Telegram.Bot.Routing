using Telegram.Bot.Routing.Utils;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Routers;

public abstract class BaseRouter
{
    protected static InlineKeyboardButton Button(string text, string action, Action<CallbackData>? modify = null)
    {
        var callbackData = CallbackData.Create(action);
        modify?.Invoke(callbackData);
        return InlineKeyboardButton.WithCallbackData(text, callbackData.ToString());
    }
}