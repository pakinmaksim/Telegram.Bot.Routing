using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts.Messages;

public interface ITelegramMessageContext : ITelegramChatContext
{
    /// <summary>
    /// Message model from storage
    /// </summary>
    public IMessage Message { get; }
    
    /// <summary>
    /// Sending new message or editing current created message. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="messageStructure">Message structure to set</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> ShowMessage(
        MessageStructure messageStructure,
        CancellationToken ct = default);

    /// <summary>
    /// Changing the keyboard of the current message while saving the current text. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="keyboard">This object represents an inline keyboard that appears right next to the Message it belongs to</param>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> SetKeyboard(
        InlineKeyboardMarkup keyboard,
        CancellationToken ct = default);

    /// <summary>
    /// Removing the keyboard of the current message while saving the current text. The changed origin message is saved to the storage
    /// </summary>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    /// <returns>Stored message model</returns>
    public Task<IMessage> RemoveKeyboard(
        CancellationToken ct = default);
    
    /// <summary>
    /// Set current message router without calling Index method in MessageRouter
    /// </summary>
    /// <param name="routerName">Router name to set</param>
    /// <param name="routerData">Router data to set</param>
    public void SetMessageRouter(string? routerName, object? routerData);
    
    /// <summary>
    /// Set current message router without calling Index method in MessageRouter
    /// </summary>
    /// <param name="routerData">Router data to set</param>
    public void SetMessageRouter<TMessageRouter>(object? routerData = null)
        where TMessageRouter : MessageRouter;

    /// <summary>
    /// Deserialize current message router data
    /// </summary>
    /// <returns>Deserialized data if it can be deserialized</returns>
    public T? GetMessageRouterData<T>();

    /// <summary>
    /// Serialize current message router data
    /// </summary>
    /// <param name="routerData">Router data to set</param>
    public void SetMessageRouterData(object? routerData);

    /// <summary>
    /// Saves current Message to storage
    /// </summary>
    /// <param name="ct">Propagates notification that operations should be canceled</param>
    public Task SaveMessage(CancellationToken ct = default);
}