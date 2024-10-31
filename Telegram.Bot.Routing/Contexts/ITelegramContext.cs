using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Routing.Storage.Serializers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts;

public interface ITelegramContext
{
    public IServiceProvider ServiceProvider { get; }
    public TelegramBotRoutingSystem Routing { get; }
    public ITelegramBotClient Bot { get; }
    public ITelegramStorage Storage { get; }
    public IRouteDataSerializer Serializer { get; }

    public Update? Update { get; }
    
    /// <summary>
    /// Send text messages
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat</param>
    /// <param name="messageStructure">Message structure to send</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model if message was sent</returns>
    public Task<IMessage> SendMessage(
        long chatId,
        MessageStructure messageStructure,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Construct message from router and send it
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat</param>
    /// <param name="routerName">Router name to construct message</param>
    /// <param name="routerData">Router data to construct message</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model if message was sent</returns>
    public Task<IMessage?> SendRouterMessage(
        long chatId,
        string routerName,
        object? routerData = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Changing the message structure. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="messageStructure">Message structure to set</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> EditMessage(
        long chatId, 
        int messageId, 
        MessageStructure messageStructure,
        string? routerName = null, 
        object? routerData = null, 
        CancellationToken ct = default);
        
    /// <summary>
    /// Changing the keyboard of the message while saving the current text. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="keyboard">This object represents an inline keyboard that appears right next to the Message it belongs to</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> SetKeyboard(
        long chatId, 
        int messageId, 
        InlineKeyboardMarkup keyboard, 
        string? routerName = null, 
        object? routerData = null, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Removing the keyboard of the message while saving the current text. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> RemoveKeyboard(
        long chatId, 
        int messageId, 
        string? routerName = null, 
        object? routerData = null, 
        CancellationToken ct = default);
    
    /// <summary>
    /// The origin user is stored in the storage as up-to-date
    /// </summary>
    /// <param name="origin">Received user</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored user model</returns>
    public Task<IUser> ReconstructUser(User origin, CancellationToken ct = default);
        
    /// <summary>
    /// The origin chat is stored in the storage as up-to-date without changing the name and data of the router and route
    /// </summary>
    /// <param name="origin">Received chat</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored chat model</returns>
    public Task<IChat> ReconstructChat(Chat origin, CancellationToken ct = default);
        
    /// <summary>
    /// The origin chat is stored in the storage as up-to-date
    /// </summary>
    /// <param name="origin">Received chat</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="routeName">Route name to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored chat model</returns>
    public Task<IChat> ReconstructChat(Chat origin, string? routerName, string? routerData, string? routeName, CancellationToken ct = default);
    
    /// <summary>
    /// The origin message is stored in the storage as up-to-date without changing the name and data of the router
    /// </summary>
    /// <param name="origin">Received message</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> ReconstructMessage(Message origin, CancellationToken ct = default);
    
    /// <summary>
    /// The origin message is stored in the storage as up-to-date
    /// </summary>
    /// <param name="origin">Received message</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> ReconstructMessage(Message origin, string? routerName, object? routerData, CancellationToken ct = default);
}