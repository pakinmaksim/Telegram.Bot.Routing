using System.Text.Json;
using Telegram.Bot.Routing.Contexts.BotMessages;
using Telegram.Bot.Routing.Contexts.Chats;

namespace Telegram.Bot.Routing.Routers;

public abstract class ChatRouter : BaseRouter
{
    public virtual Task OnRouteIn(ChatContext context, CancellationToken ct = default) 
        => Task.CompletedTask;
    public virtual Task OnUserMessage(UserMessageContext context, CancellationToken ct = default) 
        => Task.CompletedTask;
    public virtual Task OnCallbackQuery(CallbackQueryContext context, CancellationToken ct = default)
        => Task.CompletedTask;
    public virtual Task OnRouteOut(ChatContext context, CancellationToken ct = default) 
        => Task.CompletedTask;
}

public abstract class ChatRouter<TData> : ChatRouter
    where TData : class, new()
{
    private bool IsChatDataDeserialized { get; set; }
    private object? DeserializedChatData { get; set; }
    
    public override async Task OnRouteIn(ChatContext context, CancellationToken ct = default)
        => await UsingChatData(OnRouteIn, context, ct);
    public virtual Task OnRouteIn(ChatContext context, TData data, CancellationToken ct = default)
        => Task.CompletedTask;

    public override async Task OnUserMessage(UserMessageContext context, CancellationToken ct = default)
        => await UsingChatData(OnUserMessage, context, ct);
    public virtual Task OnUserMessage(UserMessageContext context, TData data, CancellationToken ct = default)
        => Task.CompletedTask;

    public override async Task OnCallbackQuery(CallbackQueryContext context, CancellationToken ct = default)
        => await UsingChatData(OnCallbackQuery, context, ct);
    public virtual Task OnCallbackQuery(CallbackQueryContext context, TData data, CancellationToken ct = default)
        => Task.CompletedTask;

    public override async Task OnRouteOut(ChatContext context, CancellationToken ct = default)
        => await UsingChatData(OnRouteOut, context, ct);
    public virtual Task OnRouteOut(ChatContext context, TData data, CancellationToken ct = default)
        => Task.CompletedTask;
    
    protected async Task UsingChatData<TContext>(
        Func<TContext, TData, CancellationToken, Task> handler,
        TContext context,
        CancellationToken ct)
        where TContext : ChatContext
    {
        try
        {
            var data = DeserializeChatData(context);
            await handler(context, data, ct);
        }
        finally
        {
            SerializeChatData(context);
        }
    }
    private TData DeserializeChatData(ChatContext context)
    {
        if (!IsChatDataDeserialized || DeserializedChatData is not TData chatData)
        {
            chatData = context.ChatModel.Data?.RootElement.Deserialize<TData>() ?? new TData();
            DeserializedChatData = chatData;
        }
        IsChatDataDeserialized = true;
        return chatData;
    }
    private void SerializeChatData(ChatContext context)
    {
        if (!context.IsChatRouter(GetType())) return;
        if (IsChatDataDeserialized == false) return;
        IsChatDataDeserialized = false;
        context.ChatModel.Data = JsonSerializer.SerializeToDocument(DeserializedChatData);
        DeserializedChatData = null;
    }
}