using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.Chats;

public interface ITelegramChatContext : ITelegramContext
{
    /// <summary>
    /// Chat model from storage
    /// </summary>
    public IChat Chat { get; }
    
    /// <summary>
    /// Send text messages to the current chat
    /// </summary>
    /// <param name="messageStructure">Message structure to send</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model if message was sent</returns>
    public Task<IMessage> SendMessage(
        MessageStructure messageStructure,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default);

    /// <summary>
    /// Construct message from router and send it to the current chat
    /// </summary>
    /// <param name="routerName">Router name to construct message</param>
    /// <param name="routerData">Router data to construct message</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model if message was sent</returns>
    public Task<IMessage?> SendRouterMessage(
        string routerName,
        object? routerData = null,
        CancellationToken ct = default);

    /// <summary>
    /// Construct message from router and send it to the current chat
    /// </summary>
    /// <param name="routerData">Router data to construct message</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model if message was sent</returns>
    public Task<IMessage?> SendRouterMessage<TMessageRouter>(
        object? routerData = null,
        CancellationToken ct = default)
        where TMessageRouter : MessageRouter;

    /// <summary>
    /// Changing the message structure in current chat. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="messageStructure">Message structure to set</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> EditMessage(
        int messageId,
        MessageStructure messageStructure,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default);

    /// <summary>
    /// Changing the keyboard of the message in current chat while saving the current text. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="keyboard">This object represents an inline keyboard that appears right next to the Message it belongs to</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> SetKeyboard(
        int messageId,
        InlineKeyboardMarkup keyboard,
        string? routerName = null,
        object? routerData = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Removing the keyboard of message in the current chat while saving the current text. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> RemoveKeyboard(
        int messageId, 
        string? routerName = null, 
        object? routerData = null, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Construct current chat router from dependency injection
    /// </summary>
    /// <returns>Current ChatRouter</returns>
    public ChatRouter? GetChatRouter();

    /// <summary>
    /// Check current chat router by name
    /// </summary>
    /// <param name="routerName">Router name to set</param>
    public bool IsChatRouter(
        string? routerName);

    /// <summary>
    /// Check current chat router by type
    /// </summary>
    public bool IsChatRouter<TChatRouter>()
        where TChatRouter : ChatRouter;
    
    /// <summary>
    /// Set current chat router and invoke Index method in ChatRouter
    /// </summary>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    public Task<ChatRouter?> ChangeChatRouter(
        string? routerName, 
        object? routerData = null, 
        CancellationToken ct = default);

    /// <summary>
    /// Set current chat router and invoke Index method in ChatRouter
    /// </summary>
    /// <param name="routerData">Router data to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    public Task<TChatRouter> ChangeChatRouter<TChatRouter>(
        object? routerData = null, 
        CancellationToken ct = default) 
        where TChatRouter : ChatRouter;
    
    /// <summary>
    /// Removes router for current chat
    /// </summary>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    public Task RemoveChatRouter(
        CancellationToken ct = default);
    
    /// <summary>
    /// Set current chat route to handle next user messages
    /// </summary>
    /// <param name="routeName">Route to set</param>
    public void SetChatRoute(
        string? routeName);

    /// <summary>
    /// Deserialize current chat router data
    /// </summary>
    /// <returns>Deserialized data if it can be deserialized</returns>
    public T? GetChatRouterData<T>();

    /// <summary>
    /// Serialize current chat router data
    /// </summary>
    /// <param name="routerData">Router data to set</param>
    public void SetChatRouterData(
        object? routerData);

    /// <summary>
    /// Deserialize current and serialize updated chat router data
    /// </summary>
    /// <returns>Deserialized data</returns>
    public T? UpdateChatRouterData<T>(Action<T> action);

    /// <summary>
    /// Saves current Chat to storage
    /// </summary>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    public Task SaveChat(
        CancellationToken ct = default);
}