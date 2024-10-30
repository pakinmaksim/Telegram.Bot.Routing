using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Routing.Storage.Serializers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.Messages;

public class TelegramMessageContext : TelegramChatContext
{
    private bool _isMessageSaved = true;
    public IMessage Message { get; private set; } = null!;
    
    public TelegramMessageContext(
        IServiceProvider serviceProvider, 
        TelegramBotRoutingSystem routing, 
        ITelegramBotClient bot, 
        ITelegramStorage storage,
        IRouteDataSerializer serializer)
        : base(serviceProvider, routing, bot, storage, serializer)
    {
        
    }
    
    internal async Task InitializeMessage(int messageId, CancellationToken ct = default)
    {
        var exists = await Storage.GetMessage(Chat.TelegramId, messageId, ct);
        Message = exists ?? throw new ArgumentException($"Message with ID = {messageId} in chat with ID = {Chat.TelegramId} doesn't exist");
    }
    
    internal async Task InitializeMessage(Message message, CancellationToken ct = default)
    {
        Message = await ReconstructMessage(message, ct);
    }
    
    internal async Task InitializeMessage(string routerName, object? routerData, CancellationToken ct = default)
    {
        Message = await Storage.ConstructMessage(
            chatId: Chat.TelegramId, 
            messageId: IMessage.CreationId,
            routerName: routerName,
            routerData: Serializer.SerializeNullable(routerData),
            ct: ct);
    }
    
    public async Task<IMessage> ShowMessage(
        MessageStructure messageStructure,
        CancellationToken ct = default)
    {
        IMessage result;
        if (Message.IsCreated)
        {
            result = await base.EditMessage(
                chatId: Chat.TelegramId,
                messageId: Message.TelegramId,
                messageStructure: messageStructure,
                routerName: Message.RouterName,
                routerData: Message.RouterData,
                ct: ct
            );
        }
        else
        {
            result = await base.SendMessage(
                chatId: Chat.TelegramId,
                messageStructure: messageStructure,
                routerName: Message.RouterName,
                routerData: Message.RouterData,
                ct: ct
            );
        }
        Message = result;
        _isMessageSaved = false;
        return Message;
    }

    public async Task<IMessage> SetKeyboard(
        InlineKeyboardMarkup keyboard,
        CancellationToken ct = default)
    {
        if (!Message.IsCreated)
            throw new InvalidOperationException("Can't set keyboard on message, that not created yet");
        
        var result = await base.SetKeyboard(
            chatId: Chat.TelegramId,
            messageId: Message.TelegramId,
            keyboard: keyboard,
            routerName: Message.RouterName,
            routerData: Message.RouterData,
            ct: ct
        );
        Message = result;
        _isMessageSaved = false;
        return Message;
    }
    
    public void SetMessageRouter(string? routerName, object? routerData)
    {
        Message.RouterName = routerName;
        Message.RouterData = Serializer.SerializeNullable(routerData);
        _isMessageSaved = false;
    }
    
    public void SetMessageRouter<TMessageRouter>(object? routerData = null)
        where TMessageRouter : MessageRouter
    {
        var routerName = Routing.GetChatRouterName(typeof(TMessageRouter));
        SetMessageRouter(routerName, routerData);
    }
    
    public T? GetMessageRouterData<T>()
    {
        return (T?) Serializer.DeserializeNullable(Message.RouterData, typeof(T));
    }
    
    public void SetMessageRouterData(object? routerData)
    {
        Message.RouterData = Serializer.SerializeNullable(routerData);
        _isMessageSaved = false;
    }

    public async Task SaveMessage(CancellationToken ct = default)
    {
        if (!Message.IsCreated) return;
        if (_isMessageSaved) return;
        await Storage.SetMessage(Message, ct);
        _isMessageSaved = true;
    }
}