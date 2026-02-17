using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Core.Messages;
using Telegram.Bot.Routing.Registration;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Routing.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.Chats;

public class ChatContext : TelegramContext
{
    public IChat ChatModel { get; internal set; } = null!;
    
    internal async Task InitializeChat(Chat chat, CancellationToken ct = default)
    {
        ChatModel = await GetChatModel(chat, ct);
    }
    internal async Task InitializeChat(long chatId, CancellationToken ct = default)
    {
        var chat = await GetChatModel(chatId, ct);
        ChatModel = chat ?? throw new ArgumentException($"Chat with Id = {chatId} not found");
    }
    

    internal override async Task Store(CancellationToken ct = default)
    {
        await Scope.UpdateChat(ChatModel, null, ct);
    }
    
    public bool IsChatRouter<TRouter>() where TRouter : ChatRouter
    {
        return System.GetChatRouterName(ChatModel.Router) == System.GetChatRouterName<TRouter>();
    }
    public bool IsChatRouter(Type type)
    {
        return System.GetChatRouterName(ChatModel.Router) == System.GetChatRouterName(type);
    }
    public bool IsChatRouter(string? name)
    {
        return System.GetChatRouterName(ChatModel.Router) == System.GetChatRouterName(name);
    }
    public ChatRouter? GetCurrentChatRouter()
    {
        return Scope.GetChatRouter(ChatModel.Router);
    }
    public async Task<TRouter> RouteChatTo<TRouter, TData>(TData data, CancellationToken ct = default)
        where TRouter : ChatRouter<TData>
        where TData : class, new()
    {
        var router = System.GetChatRouterName<TRouter>();
        return (await RouteChatTo(router, data, ct) as TRouter)!;
    }
    public async Task<TRouter> RouteChatTo<TRouter>(CancellationToken ct = default)
        where TRouter : ChatRouter
    {
        var router = System.GetChatRouterName<TRouter>();
        return (await RouteChatTo(router, null, ct) as TRouter)!;
    }
    public async Task<ChatRouter?> RouteChatTo(Type routerType, object? data = null, CancellationToken ct = default)
    {
        var name = System.GetChatRouterName(routerType);
        return await RouteChatTo(name, data, ct);
    }
    public async Task<ChatRouter?> RouteChatTo(string? name, object? data = null, CancellationToken ct = default)
    {
        var currentRouter = GetCurrentChatRouter();
        if (ChatModel.Router == name) return currentRouter;
        
        if (currentRouter is not null)
            await currentRouter.OnRouteOut(this, ct);
        
        ChatModel.Router = name;
        ChatModel.Data = JsonSerializer.SerializeToDocument(data, System.Config.JsonSerializerOptions);
        
        currentRouter = GetCurrentChatRouter();
        if (currentRouter is not null) 
            await currentRouter.OnRouteIn(this, ct);

        return currentRouter;
    }
    
    
    public async Task<IMessage?> SendMessage<TRouter, TData>(
        TData data,
        CancellationToken ct = default)
        where TRouter : BotMessageRouter<TData>
        where TData : class, new()
    {
        return await SendMessageProtected<TRouter>(data, ct);
    }
    
    public async Task<IMessage?> SendMessage<TRouter>(
        CancellationToken ct = default)
        where TRouter : BotMessageRouter
    {
        return await SendMessageProtected<TRouter>(null, ct);
    }
    
    protected async Task<IMessage?> SendMessageProtected<TRouter>(
        object? data = null,
        CancellationToken ct = default)
        where TRouter : BotMessageRouter
    {
        var chatId = ChatModel.TelegramId;
        var routerName = System.GetBotMessageRouterName<TRouter>();
        if (routerName is null) return null;
        
        var context = await Scope.CreateNewBotMessageContext(chatId, routerName, data, ct);
        var router = context.GetCurrentBotMessageRouter();
        if (router is null) return null;
        
        var message = await router.OnRouteIn(context, ct);
        if (message == null) return null;
        
        var model = await SendMessageProtected(
            chatId: ChatModel.TelegramId,
            message: message.ToNewBotMessage(),
            router: context.BotMessageModel.Router,
            data: context.BotMessageModel.Data,
            ct: ct);

        return model;
    }
    
    public async Task<IMessage?> ShowMessage<TRouter, TData>(
        TData data,
        CancellationToken ct = default)
        where TRouter : BotMessageRouter<TData>
        where TData : class, new()
    {
        return await ShowMessageProtected<TRouter>(data, ct);
    }
    
    public async Task<IMessage?> ShowMessage<TRouter>(
        CancellationToken ct = default)
        where TRouter : BotMessageRouter
    {
        return await ShowMessageProtected<TRouter>(null, ct);
    }
    
    protected virtual async Task<IMessage?> ShowMessageProtected<TRouter>(
        object? data = null,
        CancellationToken ct = default)
        where TRouter : BotMessageRouter
    {
        return await SendMessageProtected<TRouter>(data, ct);
    }
    
    public async Task<IMessage?> ShowMessage(
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        ParseMode? parseMode = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        IEnumerable<MessageEntity>? entities = null,
        string? businessConnectionId = null,
        CancellationToken ct = default)
    {
        return await ShowMessage(
            message: new ShowBotMessage()
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
    
    public virtual async Task<IMessage?> ShowMessage(
        ShowBotMessage message,
        CancellationToken ct = default)
    {
        return await SendMessage(message.ToNewBotMessage(), ct);
    }
    
    public async Task<IMessage> SendMessage(
        string text,
        ReplyMarkup? replyMarkup = null,
        ParseMode? parseMode = null,
        ReplyParameters? replyParameters = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        int? messageThreadId = null,
        IEnumerable<MessageEntity>? entities = null,
        bool disableNotification = false,
        bool protectContent = false,
        string? messageEffectId = null,
        string? businessConnectionId = null,
        bool allowPaidBroadcast = false,
        long? directMessagesTopicId = null,
        SuggestedPostParameters? suggestedPostParameters = null,
        CancellationToken ct = default)
    {
        return await SendMessage(
            message: new NewBotMessage()
            {
                Text = text,
                ReplyMarkup = replyMarkup,
                ParseMode = parseMode,
                ReplyParameters = replyParameters,
                LinkPreviewOptions = linkPreviewOptions,
                MessageThreadId = messageThreadId,
                Entities = entities,
                DisableNotification = disableNotification,
                ProtectContent = protectContent,
                MessageEffectId = messageEffectId,
                BusinessConnectionId = businessConnectionId,
                AllowPaidBroadcast = allowPaidBroadcast,
                DirectMessagesTopicId = directMessagesTopicId,
                SuggestedPostParameters = suggestedPostParameters
            },
            ct: ct);
    }
    
    public async Task<IMessage> SendMessage(
        NewBotMessage message,
        CancellationToken ct = default)
    {
        return await SendMessageProtected(
            chatId: ChatModel.TelegramId,
            message: message,
            router: ChatModel.Router,
            data: null,
            ct: ct);
    }
    
    public async Task DeleteMessage(int messageId, CancellationToken ct = default)
    {
        await System.Bot.DeleteMessage(
            chatId: ChatModel.TelegramId,
            messageId: messageId,
            cancellationToken: ct);
    }
}