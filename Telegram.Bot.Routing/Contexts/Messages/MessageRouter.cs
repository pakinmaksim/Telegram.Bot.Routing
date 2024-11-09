using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.Messages;

/// <summary>
/// Base class for all message routers
/// </summary>
public abstract class MessageRouter
{
    public ITelegramMessageContext Context { get; private set; } = null!;
    
    public async Task InvokeIndex(CancellationToken ct = default)
    {
        await Context.Routing.InvokeMessageRouterIndex(this, ct);
    }
    
    public async Task InvokeCallbackRoute(CallbackQuery? callbackQuery, CancellationToken ct = default)
    {
        await Context.Routing.InvokeCallbackRoute(this, callbackQuery, ct);
    }

    protected InlineKeyboardButton Action(string text, CallbackData callbackData) => 
        InlineKeyboardButton.WithCallbackData(text, callbackData.ToString());

    protected InlineKeyboardButton Action(string text, string route) => 
        Action(text, CallbackData.Create(route));
}