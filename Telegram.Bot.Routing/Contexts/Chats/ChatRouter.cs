using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Contexts.Chats;

/// <summary>
/// Base class for all chat routers
/// </summary>
public abstract class ChatRouter
{
    public ITelegramChatContext Context { get; private set; } = null!;

    public async Task InvokeIndex(CancellationToken ct = default)
    {
        await Context.Routing.InvokeChatRouterIndex(this, ct);
    }
    
    public async Task InvokeMessageRoute(Message? message, CancellationToken ct = default)
    {
        await Context.Routing.InvokeMessageRoute(this, message, ct);
    }
}