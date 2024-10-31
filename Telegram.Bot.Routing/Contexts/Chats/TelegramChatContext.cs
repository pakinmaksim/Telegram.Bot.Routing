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

    public void SetChatRouter(string? routerName, object? routerData = null)
    {
        Chat.RouterName = routerName;
        Chat.RouterData = Serializer.SerializeNullable(routerData);
        Chat.RouteName = null;
        _isChatSaved = false;
    }
    
    public void SetChatRouter<TChatRouter>(object? routerData = null)
        where TChatRouter : ChatRouter
    {
        var routerName = Routing.GetChatRouterName(typeof(TChatRouter));
        SetChatRouter(routerName, routerData);
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
    
    public async Task SaveChat(CancellationToken ct = default)
    {
        if (_isChatSaved) return;
        await Storage.SetChat(Chat, ct);
        _isChatSaved = true;
    }
}