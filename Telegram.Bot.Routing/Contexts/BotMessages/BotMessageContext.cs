using System.Text.Json;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Core.Messages;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.BotMessages;

public class BotMessageContext : ChatContext
{
    public IMessage BotMessageModel { get; internal set;} = null!;
    
    internal async Task InitializeMessage(Message botMessage, CancellationToken ct = default)
    {
        BotMessageModel = await GetMessageModel(botMessage, ct);
    }
    internal async Task InitializeMessage(int messageId, CancellationToken ct = default)
    {
        var model = await GetMessageModel(ChatModel.TelegramId, messageId, ct);
        if (model is null)
            throw new ArgumentException($"Message with Id = ({ChatModel.TelegramId}, {messageId}) not found");
        BotMessageModel = model;
    }
    internal void InitializeUnsentMessage(string router, object? data)
    {
        var dataSerialized = JsonSerializer.SerializeToDocument(data, System.Config.JsonSerializerOptions);
        BotMessageModel = new UnsendMessage()
        {
            TelegramChatId = ChatModel.TelegramId,
            TelegramMessageId = UnsendMessage.UnsentMessageId,
            TelegramFromId = System.Bot.BotId,
            Date = default,
            Router = router,
            Data = dataSerialized
        };
    }

    public override async Task<IMessage?> GetMessageModel(long chatId, int messageId, CancellationToken ct = default)
    {
        if (BotMessageModel?.TelegramChatId == chatId && BotMessageModel?.TelegramMessageId == messageId)
            return BotMessageModel;
        return await base.GetMessageModel(chatId, messageId, ct);
    }

    internal override async Task Store(CancellationToken ct = default)
    {
        await base.Store(ct);
        await Scope.UpdateMessage(BotMessageModel, null, ct);
    }
    
    public bool IsBotMessageRouter<TRouter>() where TRouter : BotMessageRouter
    {
        return System.GetBotMessageRouterName(BotMessageModel.Router) == System.GetBotMessageRouterName<TRouter>();
    }
    public bool IsBotMessageRouter(Type type)
    {
        return System.GetBotMessageRouterName(BotMessageModel.Router) == System.GetBotMessageRouterName(type);
    }
    public bool IsBotMessageRouter(string? name)
    {
        return System.GetBotMessageRouterName(BotMessageModel.Router) == System.GetBotMessageRouterName(name);
    }
    public BotMessageRouter? GetCurrentBotMessageRouter()
    {
        return Scope.GetBotMessageRouter(BotMessageModel.Router);
    }
    public async Task<TRouter> RouteBotMessageTo<TRouter, TData>(TData data, CancellationToken ct = default)
        where TRouter : BotMessageRouter<TData>
        where TData : class, new()
    {
        var router = System.GetBotMessageRouterName<TRouter>();
        return (await RouteBotMessageTo(router, data, ct) as TRouter)!;
    }
    public async Task<TRouter> RouteBotMessageTo<TRouter>(CancellationToken ct = default)
        where TRouter : BotMessageRouter
    {
        var router = System.GetBotMessageRouterName<TRouter>();
        return (await RouteBotMessageTo(router, null, ct) as TRouter)!;
    }
    public async Task<BotMessageRouter?> RouteBotMessageTo(Type routerType, object? data = null, CancellationToken ct = default)
    {
        var name = System.GetBotMessageRouterName(routerType);
        return await RouteBotMessageTo(name, data, ct);
    }
    public async Task<BotMessageRouter?> RouteBotMessageTo(string? name, object? data = null, CancellationToken ct = default)
    {
        var currentRouter = GetCurrentBotMessageRouter();
        if (IsBotMessageRouter(name)) return currentRouter;
        
        if (currentRouter is not null)
            await currentRouter.OnRouteOut(this, ct);
        
        BotMessageModel.Router = name;
        BotMessageModel.Data = JsonSerializer.SerializeToDocument(data, System.Config.JsonSerializerOptions);
        
        currentRouter = GetCurrentBotMessageRouter();
        if (currentRouter is not null) 
            await currentRouter.OnRouteIn(this, ct);

        return currentRouter;
    }
    
    protected override async Task<IMessage?> ShowMessageProtected<TRouter>(
        object? data = null,
        CancellationToken ct = default)
    {
        if (BotMessageModel is UnsendMessage)
        {
            return await SendMessageProtected<TRouter>(data, ct);
        }
        else
        {
            return await EditMessageProtected<TRouter>(data, ct);
        }
    }
    
    public override async Task<IMessage?> ShowMessage(
        ShowBotMessage message,
        CancellationToken ct = default)
    {
        if (BotMessageModel is UnsendMessage)
        {
            return await SendMessage(message.ToNewBotMessage(), ct);
        }
        else
        {
            return await EditMessage(message.ToEditBotMessage(), ct);
        }
    }
    
    public async Task<IMessage?> EditMessage<TRouter, TData>(
        TData data,
        CancellationToken ct = default)
        where TRouter : BotMessageRouter<TData>
        where TData : class, new()
    {
        return await EditMessageProtected<TRouter>(data, ct);
    }
    
    public async Task<IMessage?> EditMessage<TRouter>(
        CancellationToken ct = default)
        where TRouter : BotMessageRouter
    {
        return await EditMessageProtected<TRouter>(null, ct);
    }
    
    protected async Task<IMessage?> EditMessageProtected<TRouter>(
        object? data = null,
        CancellationToken ct = default)
        where TRouter : BotMessageRouter
    {
        if (BotMessageModel is UnsendMessage)
            throw new InvalidOperationException($"Cannot edit message because it is unsent");

        var nextRouterName = System.GetBotMessageRouterName<TRouter>();
        var isRouterChanged = !IsBotMessageRouter(nextRouterName);

        var router = GetCurrentBotMessageRouter();
        if (isRouterChanged && router is not null)
            await router.OnRouteOut(this, ct);
        
        var nextRouter = Scope.GetBotMessageRouter(nextRouterName);
        BotMessageModel.Router = nextRouterName;
        BotMessageModel.Data = JsonSerializer.SerializeToDocument(data, System.Config.JsonSerializerOptions);
        var message = nextRouter is not null ? await nextRouter.OnRouteIn(this, ct) : null;
        if (message is null) return null;
        
        var model = await EditMessageProtected(
            chatId: BotMessageModel.TelegramChatId,
            messageId: BotMessageModel.TelegramMessageId,
            message: message.ToEditBotMessage(),
            router: BotMessageModel.Router,
            data: BotMessageModel.Data,
            ct: ct);

        return model;
    }
    
    public async Task<IMessage> EditMessage(
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        ParseMode? parseMode = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        IEnumerable<MessageEntity>? entities = null,
        string? businessConnectionId = null,
        CancellationToken ct = default)
    {
        return await EditMessage(
            message: new EditBotMessage()
            {
                Text = text,
                ReplyMarkup = replyMarkup,
                ParseMode = parseMode,
                LinkPreviewOptions = linkPreviewOptions,
                Entities = entities,
                BusinessConnectionId = businessConnectionId
            },
            ct: ct);
    }
    
    public async Task<IMessage> EditMessage(
        EditBotMessage message,
        CancellationToken ct = default)
    {
        if (BotMessageModel is UnsendMessage)
            throw new InvalidOperationException($"Cannot edit message because it is unsent");
        
        return await EditMessageProtected(
            chatId: BotMessageModel.TelegramChatId,
            messageId: BotMessageModel.TelegramMessageId,
            message: message,
            router: BotMessageModel.Router,
            data: BotMessageModel.Data,
            ct: ct);
    }
    
    public async Task<IMessage> EditReplyMarkup(
        InlineKeyboardMarkup replyMarkup,
        string? businessConnectionId = null,
        CancellationToken ct = default)
    {
        if (BotMessageModel is UnsendMessage)
            throw new InvalidOperationException($"Cannot edit message because it is unsent");
        
        var message = await System.Bot.EditMessageReplyMarkup(
            chatId: BotMessageModel.TelegramChatId,
            messageId: BotMessageModel.TelegramMessageId,
            replyMarkup: replyMarkup,
            businessConnectionId: businessConnectionId,
            cancellationToken: ct);
        await Scope.UpdateMessage(BotMessageModel, message, ct);
        return BotMessageModel;
    }
}