using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Routing.Storage.Serializers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts;

public class TelegramContext : ITelegramContext
{
    public IServiceProvider ServiceProvider { get; private set; }
    public TelegramBotRoutingSystem Routing { get; private set; }
    public ITelegramBotClient Bot { get; private set; }
    public ITelegramStorage Storage { get; private set; }
    public IRouteDataSerializer Serializer { get; private set; }

    public Update? Update { get; internal set; }
    
    public TelegramContext(
        IServiceProvider serviceProvider,
        TelegramBotRoutingSystem routing,
        ITelegramBotClient bot,
        ITelegramStorage storage,
        IRouteDataSerializer serializer)
    {
        ServiceProvider = serviceProvider;
        Routing = routing;
        Bot = bot;
        Storage = storage;
        Serializer = serializer;
    }

    public async Task<IMessage> SendMessage(
        long chatId,
        MessageStructure messageStructure,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default)
    {
        var message = await Bot.SendTextMessageAsync(
            chatId: chatId,
            text: messageStructure.Text,
            replyMarkup: messageStructure.ReplyMarkup,
            parseMode: Routing.Config.DefaultParseMode,
            cancellationToken: ct);
        return await ReconstructMessage(message, routerName, routerData, ct);
    }
    
    protected async Task<IMessage?> SendRouterMessage(
        AsyncServiceScope scope,
        CancellationToken ct = default)
    {
        var context = scope.ServiceProvider.GetRequiredService<TelegramMessageContext>();
        var result = await Routing.InvokeMessageContextIndex(context, ct);
        await context.SaveMessage(ct);
        await context.SaveChat(ct);
        return result as IMessage;
    }

    public virtual async Task<IMessage?> SendRouterMessage(
        long chatId,
        string routerName,
        object? routerData = null,
        CancellationToken ct = default)
    {
        await using var scope = await Routing.CreateNewMessageScope(chatId, routerName, routerData, ct);
        return await SendRouterMessage(scope, ct);
    }

    public async Task<IMessage> EditMessage(
        long chatId,
        int messageId,
        MessageStructure messageStructure,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default)
    {
        var message = await Bot.EditMessageTextAsync(
            chatId: chatId,
            messageId: messageId,
            text: messageStructure.Text,
            replyMarkup: messageStructure.ReplyMarkup,
            parseMode: Routing.Config.DefaultParseMode,
            cancellationToken: ct);
        return await ReconstructMessage(message, routerName, routerData, ct);
    }

    public async Task<IMessage> SetKeyboard(
        long chatId,
        int messageId,
        InlineKeyboardMarkup keyboard,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default)
    {
        var message = await Bot.EditMessageReplyMarkupAsync(
            chatId: chatId,
            messageId: messageId,
            replyMarkup: keyboard,
            cancellationToken: ct);
        return await ReconstructMessage(message, routerName, routerData, ct);
    }

    public async Task<IMessage> RemoveKeyboard(
        long chatId, 
        int messageId, 
        string? routerName = null, 
        object? routerData = null,
        CancellationToken ct = default)
    {
        return await SetKeyboard(
            chatId: chatId,
            messageId: messageId,
            keyboard: new InlineKeyboardMarkup(Array.Empty<InlineKeyboardButton[]>()),
            routerName: routerName,
            routerData: routerData,
            ct: ct);
    }

    public async Task<IUser> ReconstructUser(
        User origin,
        CancellationToken ct = default)
    {
        var constructed = await Storage.ConstructUser(origin, ct);
        await Storage.SetUser(constructed, ct);
        return constructed;
    }
    
    public async Task<IChat> ReconstructChat(Chat origin, CancellationToken ct = default)
    {
        var exists = await Storage.GetChat(origin.Id, ct);
        
        string? routerName;
        string? routerData;
        string? routeName;
        if (exists != null)
        {
            routerName = exists.RouterName;
            routerData = exists.RouterData;
            routeName = exists.RouteName;
        }
        else
        {
            routerName = Routing.Config.DefaultChatRouterName;
            routerData = null;
            routeName = null;
        }
        
        return await ReconstructChat(origin, routerName, routerData, routeName, ct);
    }

    public async Task<IChat> ReconstructChat(
        Chat origin,
        string? routerName,
        string? routerData,
        string? routeName,
        CancellationToken ct = default)
    {
        var constructed = await Storage.ConstructChat(origin, ct);
        constructed.RouterName = routerName;
        constructed.RouterData = routerData;
        constructed.RouteName = routeName;
        await Storage.SetChat(constructed, ct);
        return constructed;
    }
    
    public async Task<IMessage> ReconstructMessage(Message origin, CancellationToken ct = default)
    {
        var exists = await Storage.GetMessage(origin.Chat.Id, origin.MessageId, ct);
        
        string? routerName;
        string? routerData;
        if (exists != null)
        {
            routerName = exists.RouterName;
            routerData = exists.RouterData;
        }
        else
        {
            routerName = Routing.Config.DefaultMessageRouterName;
            routerData = null;
        }
        
        return await ReconstructMessage(origin, routerName, routerData, ct);
    }
    
    public async Task<IMessage> ReconstructMessage(
        Message origin,
        string? routerName,
        object? routerData,
        CancellationToken ct = default)
    {
        var constructed = await Storage.ConstructMessage(origin, ct);
        constructed.RouterName = routerName;
        constructed.RouterData = Serializer.SerializeNullable(routerData);
        await Storage.SetMessage(constructed, ct);
        return constructed;
    }
}