using System.Text.Json;
using Telegram.Bot.Routing.Contexts.BotMessages;
using Telegram.Bot.Routing.Core.Messages;
using Telegram.Bot.Routing.Storage.Models;

namespace Telegram.Bot.Routing.Routers;

public abstract class BotMessageRouter : BaseRouter
{
    public abstract Task<ShowBotMessage?> OnRouteIn(BotMessageContext context, CancellationToken ct = default);
    public virtual Task OnCallbackQuery(CallbackQueryContext context, CancellationToken ct = default) 
        => Task.CompletedTask;
    public virtual Task OnRouteOut(BotMessageContext context, CancellationToken ct = default) 
        => Task.CompletedTask;
}

public abstract class BotMessageRouter<TData> : BotMessageRouter
    where TData : class, new()
{
    private bool IsMessageDataDeserialized { get; set; }
    private object? DeserializedMessageData { get; set; }
    
    public override async Task<ShowBotMessage?> OnRouteIn(BotMessageContext context, CancellationToken ct = default)
        => await UsingBotMessageDataWithResult(OnRouteIn, context, ct);
    public abstract Task<ShowBotMessage?> OnRouteIn(BotMessageContext context, TData data, CancellationToken ct = default);

    public override async Task OnCallbackQuery(CallbackQueryContext context, CancellationToken ct = default) 
        => await UsingBotMessageData(OnCallbackQuery, context, ct);
    public virtual Task OnCallbackQuery(CallbackQueryContext context, TData data, CancellationToken ct = default) 
        => Task.CompletedTask;

    public override async Task OnRouteOut(BotMessageContext context, CancellationToken ct = default) 
        => await UsingBotMessageData(OnRouteOut, context, ct);
    public virtual Task OnRouteOut(BotMessageContext context, TData data, CancellationToken ct = default) 
        => Task.CompletedTask;

    private async Task UsingBotMessageData<TContext>(
        Func<TContext, TData, CancellationToken, Task> handler,
        TContext context,
        CancellationToken ct)
        where TContext : BotMessageContext
    {
        await UsingBotMessageDataWithResult<TContext, object>(async (x, y, z) =>
        {
            await handler(x, y, z);
            return new object();
        }, context, ct);
    }
    private async Task<TResult> UsingBotMessageDataWithResult<TContext, TResult>(
        Func<TContext, TData, CancellationToken, Task<TResult>> handler,
        TContext context,
        CancellationToken ct)
        where TContext : BotMessageContext
    {
        try
        {
            var data = DeserializeBotMessageData(context);
            return await handler(context, data, ct);
        }
        finally
        {
            SerializeBotMessageData(context);
        }
    }
    private TData DeserializeBotMessageData(BotMessageContext context)
    {
        if (!IsMessageDataDeserialized || DeserializedMessageData is not TData messageData)
        {
            messageData = context.BotMessageModel.Data?.RootElement.Deserialize<TData>() ?? new TData();
            DeserializedMessageData = messageData;
        }
        IsMessageDataDeserialized = true;
        return messageData;
    }
    private void SerializeBotMessageData(BotMessageContext context)
    {
        if (!context.IsBotMessageRouter(GetType())) return;
        if (IsMessageDataDeserialized == false) return;
        IsMessageDataDeserialized = false;
        context.BotMessageModel.Data = JsonSerializer.SerializeToDocument(DeserializedMessageData);
        DeserializedMessageData = null;
    }
}