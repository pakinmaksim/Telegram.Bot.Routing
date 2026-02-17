using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Contexts.BotMessages;
using Telegram.Bot.Routing.Core;
using Telegram.Bot.Routing.Core.Messages;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Contexts;

public class TelegramContext
{
    public TelegramRoutingSystem System { get; internal set; } = null!;
    public TelegramScope Scope { get; internal set; } = null!;
    public Update? Update => Scope.Update;

    internal virtual Task Store(CancellationToken ct = default) => Task.CompletedTask;

    public async Task<IUser> GetUserModel(User origin, CancellationToken ct = default)
    {
        var userId = origin.Id;
        var stored = await Scope.GetUser(userId, ct);
        if (stored is null) stored = await Scope.UpsertUser(origin, ct);
        else await Scope.UpdateUser(stored, origin, ct);
        return stored;
    }
    public virtual async Task<IUser?> GetUserModel(long userId, CancellationToken ct = default)
    {
        return await Scope.GetUser(userId, ct);
    }

    public async Task<IChat> GetChatModel(Chat origin, CancellationToken ct = default)
    {
        var chatId = origin.Id;
        var stored = await Scope.GetChat(chatId, ct);
        if (stored is null)
        {
            stored = await Scope.UpsertChat(origin, System.Config.DefaultChatRouterName, null, ct);
        }
        else await Scope.UpdateChat(stored, origin, ct);
        return stored;
    }
    public virtual async Task<IChat?> GetChatModel(long chatId, CancellationToken ct = default)
    {
        return await Scope.GetChat(chatId, ct);
    }

    public async Task<IMessage> GetMessageModel(Message origin, CancellationToken ct = default)
    {
        var chatId = origin.Chat.Id;
        var messageId = origin.MessageId;
        var stored = await Scope.GetMessage(chatId, messageId, ct);
        if (stored is null)
        {
            var router = origin.From?.Id == System.Bot.BotId ? System.Config.DefaultBotMessageRouterName : null; 
            stored = await Scope.UpsertMessage(origin, router, null, ct);
        }
        else await Scope.UpdateMessage(stored, origin, ct);
        return stored;
    }
    public virtual async Task<IMessage?> GetMessageModel(long chatId, int messageId, CancellationToken ct = default)
    {
        return await Scope.GetMessage(chatId, messageId, ct);
    }
    
    
    protected async Task<IMessage> SendMessageProtected(
        long chatId,
        NewBotMessage message,
        string? router,
        object? data,
        CancellationToken ct = default)
    {
        var sentMessage = await System.Bot.SendMessage(
            chatId: chatId,
            text: message.Text,
            parseMode: message.ParseMode ?? System.Config.DefaultParseMode,
            replyParameters: message.ReplyParameters,
            replyMarkup: message.ReplyMarkup,
            linkPreviewOptions: message.LinkPreviewOptions,
            messageThreadId: message.MessageThreadId,
            entities: message.Entities,
            disableNotification: message.DisableNotification,
            protectContent: message.ProtectContent,
            messageEffectId: message.MessageEffectId,
            businessConnectionId: message.BusinessConnectionId,
            allowPaidBroadcast: message.AllowPaidBroadcast,
            directMessagesTopicId: message.DirectMessagesTopicId,
            suggestedPostParameters: message.SuggestedPostParameters,
            cancellationToken: ct);
        var dataSerialized = JsonSerializer.SerializeToDocument(data, System.Config.JsonSerializerOptions);
        return await Scope.UpsertMessage(sentMessage, router, dataSerialized, ct);
    }
    
    protected async Task<IMessage> EditMessageProtected(
        long chatId,
        int messageId,
        EditBotMessage message,
        string? router,
        object? data,
        CancellationToken ct = default)
    {
        var sentMessage = await System.Bot.EditMessageText(
            chatId: chatId,
            messageId: messageId,
            text: message.Text,
            parseMode: message.ParseMode ?? System.Config.DefaultParseMode,
            replyMarkup: message.ReplyMarkup,
            linkPreviewOptions: message.LinkPreviewOptions,
            entities: message.Entities,
            businessConnectionId: message.BusinessConnectionId,
            cancellationToken: ct);
        var dataSerialized = JsonSerializer.SerializeToDocument(data, System.Config.JsonSerializerOptions);
        return await Scope.UpsertMessage(sentMessage, router, dataSerialized, ct);
    }
}