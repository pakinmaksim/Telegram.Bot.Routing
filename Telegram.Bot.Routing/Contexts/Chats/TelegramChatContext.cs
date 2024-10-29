using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Routing.Storage.Serializers;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Contexts.Chats;

public class TelegramChatContext : TelegramContext
{
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
    
    internal async Task InitializeChat(long chatId, CancellationToken ct = default)
    {
        var exists = await Storage.GetChat(chatId, ct);
        Chat = exists ?? throw new ArgumentException($"Chat with ID = {chatId} doesn't exist");
    }
    
    internal async Task InitializeChat(Chat chat, CancellationToken ct = default)
    {
        Chat = await ReconstructChat(chat, ct);
    }

    public T? GetChatRouterData<T>()
    {
        return (T?) Serializer.DeserializeNullable(Chat.RouterData, typeof(T));
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
        return await base.SendRouterMessage(
            chatId: Chat.TelegramId,
            routerName: routerName,
            routerData: routerData,
            ct: ct);
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
    
    public void SetChatRouter(string? routerName, object? routerData = null)
    {
        Chat.RouterName = routerName;
        Chat.RouterData = Serializer.SerializeNullable(routerData);
        Chat.RouteName = null;
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
    }
    
    public async Task SaveChat(CancellationToken ct = default)
    {
        await Storage.SetChat(Chat, ct);
    }
}