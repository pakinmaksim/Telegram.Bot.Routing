using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Routing.Storage.Serializers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.Chats;

public class TelegramChatContext : TelegramContext, ITelegramChatContext
{
    private bool _isChatSaved = true;
    public IChat Chat { get; private set; } = null!;
    
    public TelegramChatContext(
        IServiceProvider serviceProvider, 
        TelegramBotRoutingSystem routing, 
        ITelegramBotClient bot, 
        ITelegramStorage storage,
        IRouteDataSerializer serializer)
        : base(serviceProvider, routing, bot, storage, serializer)
    {
        
    }
    
    internal void InitializeChat(IChat chat)
    {
        Chat = chat;
    }
    
    internal async Task InitializeChat(long chatId, CancellationToken ct = default)
    {
        var exists = await Storage.GetChat(chatId, ct);
        InitializeChat(exists ?? throw new ArgumentException($"Chat with ID = {chatId} doesn't exist"));
    }
    
    internal async Task InitializeChat(Chat chat, CancellationToken ct = default)
    {
        var model = await ReconstructChat(chat, ct);
        InitializeChat(model);
    }
    
    public async Task<IMessage> SendMessage(
        MessageStructure messageStructure,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default)
    {
        return await base.SendMessage(
            chatId: Chat.TelegramId,
            messageStructure: messageStructure,
            routerName: routerName,
            routerData: routerData,
            ct: ct);
    }
    
    public async Task<IMessage?> SendRouterMessage(
        string routerName,
        object? routerData = null,
        CancellationToken ct = default)
    {
        await using var scope = await Routing.CreateNewMessageScope(this, routerName, routerData, ct);
        return await base.SendRouterMessage(scope, ct);
    }

    public Task<IMessage?> SendRouterMessage<TMessageRouter>(
        object? routerData = null, 
        CancellationToken ct = default) 
        where TMessageRouter : MessageRouter
    {
        var routerName = Routing.GetMessageRouterName<TMessageRouter>();
        return SendRouterMessage(routerName, routerData, ct);
    }

    public override Task<IMessage?> SendRouterMessage(
        long chatId, 
        string routerName, 
        object? routerData = null, 
        CancellationToken ct = default)
    {
        if (chatId == Chat.TelegramId) return SendRouterMessage(routerName, routerData, ct);
        return base.SendRouterMessage(chatId, routerName, routerData, ct);
    }

    public async Task<IMessage> EditMessage(
        int messageId,
        MessageStructure messageStructure,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default)
    {
        return await base.EditMessage(
            chatId: Chat.TelegramId,
            messageId: messageId,
            messageStructure: messageStructure,
            routerName: routerName,
            routerData: routerData,
            ct: ct
        );
    }

    public async Task<IMessage> SetKeyboard(
        int messageId,
        InlineKeyboardMarkup keyboard,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default)
    {
        return await base.SetKeyboard(
            chatId: Chat.TelegramId,
            messageId: messageId,
            keyboard: keyboard,
            routerName: routerName,
            routerData: routerData,
            ct: ct);
    }

    public async Task<IMessage> RemoveKeyboard(
        int messageId, 
        string? routerName = null, 
        object? routerData = null,
        CancellationToken ct = default)
    {
        return await base.RemoveKeyboard(
            chatId: Chat.TelegramId,
            messageId: messageId,
            routerName: routerName,
            routerData: routerData,
            ct: ct);
    }

    public ChatRouter? GetChatRouter()
    {
        var routerType = Routing.GetChatRouterType(Chat.RouterName);
        if (routerType is null) return null;
        return (ChatRouter) ServiceProvider.GetRequiredService(routerType);
    }

    public bool IsChatRouter(string? routerName)
    {
        return Chat.RouterName == routerName;
    }

    public bool IsChatRouter<TChatRouter>()
        where TChatRouter : ChatRouter
    {
        var routerName = Routing.GetChatRouterName(typeof(TChatRouter));
        return IsChatRouter(routerName);
    }

    public async Task<ChatRouter?> ChangeChatRouter(string? routerName, object? routerData = null, CancellationToken ct = default)
    {
        Chat.RouterName = routerName;
        Chat.RouterData = Serializer.SerializeNullable(routerData);
        Chat.RouteName = null;
        _isChatSaved = false;
        
        var router = GetChatRouter();
        if (router is null) return null;
        await router.InvokeIndex(ct);
        return router;
    }
    
    public async Task<TChatRouter> ChangeChatRouter<TChatRouter>(object? routerData = null, CancellationToken ct = default)
        where TChatRouter : ChatRouter
    {
        var routerName = Routing.GetChatRouterName(typeof(TChatRouter));
        var router = await ChangeChatRouter(routerName, routerData, ct);
        return (TChatRouter) router!;
    }

    public async Task RemoveChatRouter(CancellationToken ct = default)
    {
        await ChangeChatRouter(null, ct: ct);
    }

    public void SetChatRoute(string? routeName)
    {
        Chat.RouteName = routeName;
        _isChatSaved = false;
    }
    
    public T? GetChatRouterData<T>()
    {
        return (T?) Serializer.DeserializeNullable(Chat.RouterData, typeof(T));
    }
    
    public void SetChatRouterData(object? routerData)
    {
        Chat.RouterData = Serializer.SerializeNullable(routerData);
        _isChatSaved = false;
    }

    public T? UpdateChatRouterData<T>(Action<T> action)
    {
        var data = GetChatRouterData<T>();
        if (data is null) return default;
        action.Invoke(data);
        SetChatRouterData(data);
        return data;
    }

    public async Task SaveChat(CancellationToken ct = default)
    {
        if (_isChatSaved) return;
        await Storage.SetChat(Chat, ct);
        _isChatSaved = true;
    }
}