using System.Collections.Concurrent;
using System.Text.Json;
using Telegram.Bot.Routing.Storage.InMemory.Models;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Storage.InMemory;

public class InMemoryTelegramStorage : ITelegramStorage
{
    private readonly ConcurrentDictionary<long, InMemoryChat> _chats = new();
    private readonly ConcurrentDictionary<long, InMemoryUser> _users = new();
    private readonly ConcurrentDictionary<(long chatId, int messageId), InMemoryMessage> _messages = new();
    
    public Task<IChat?> GetChat(long chatId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_chats.TryGetValue(chatId, out var chat) ? (IChat?)chat : null);
    }

    public Task<IChat> UpsertChat(Chat origin, string? router, JsonDocument? data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var id = origin.Id;

        var updated = _chats.AddOrUpdate(
            id,
            addValueFactory: _ =>
            {
                var created = new InMemoryChat
                {
                    TelegramId = id,
                    Router = router,
                    Data = data
                };
                ApplyOrigin(created, origin);
                return created;
            },
            updateValueFactory: (_, existing) =>
            {
                existing.Router = router;
                existing.Data = data;
                ApplyOrigin(existing, origin);
                return existing;
            });

        return Task.FromResult((IChat)updated);
    }

    public Task UpdateChat(IChat stored, Chat? origin = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Ensure it exists and then copy fields.
        var chat = _chats.AddOrUpdate(
            stored.TelegramId,
            addValueFactory: _ =>
            {
                var created = new InMemoryChat
                {
                    TelegramId = stored.TelegramId,
                    Router = stored.Router,
                    Data = stored.Data
                };
                CopyFrom(created, stored);
                if (origin is not null) ApplyOrigin(created, origin);
                return created;
            },
            updateValueFactory: (_, existing) =>
            {
                existing.Router = stored.Router;
                existing.Data = stored.Data;
                CopyFrom(existing, stored);
                if (origin is not null) ApplyOrigin(existing, origin);
                return existing;
            });

        return Task.CompletedTask;
    }

    public Task<IMessage?> GetMessage(long chatId, int messageId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(
            _messages.TryGetValue((chatId, messageId), out var msg) ? (IMessage?)msg : null
        );
    }

    public Task<IMessage> UpsertMessage(Message origin, string? router, JsonDocument? data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var chatId = origin.Chat.Id;
        var messageId = origin.MessageId;
        var key = (chatId, messageId);

        var updated = _messages.AddOrUpdate(
            key,
            addValueFactory: _ =>
            {
                var created = new InMemoryMessage
                {
                    TelegramChatId = chatId,
                    TelegramMessageId = messageId,
                    Router = router,
                    Data = data
                };
                ApplyOrigin(created, origin);
                return created;
            },
            updateValueFactory: (_, existing) =>
            {
                existing.Router = router;
                existing.Data = data;
                ApplyOrigin(existing, origin);
                return existing;
            });

        return Task.FromResult((IMessage)updated);
    }

    public Task UpdateMessage(IMessage stored, Message? origin = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var key = (stored.TelegramChatId, stored.TelegramMessageId);

        _messages.AddOrUpdate(
            key,
            addValueFactory: _ =>
            {
                var created = new InMemoryMessage
                {
                    TelegramChatId = stored.TelegramChatId,
                    TelegramMessageId = stored.TelegramMessageId,
                    Router = stored.Router,
                    Data = stored.Data
                };
                CopyFrom(created, stored);
                if (origin is not null) ApplyOrigin(created, origin);
                return created;
            },
            updateValueFactory: (_, existing) =>
            {
                existing.Router = stored.Router;
                existing.Data = stored.Data;
                CopyFrom(existing, stored);
                if (origin is not null) ApplyOrigin(existing, origin);
                return existing;
            });

        return Task.CompletedTask;
    }

    public Task<IUser?> GetUser(long userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_users.TryGetValue(userId, out var user) ? (IUser?)user : null);
    }

    public Task<IUser> UpsertUser(User origin, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var id = origin.Id;

        var updated = _users.AddOrUpdate(
            id,
            addValueFactory: _ =>
            {
                var created = new InMemoryUser { TelegramId = id };
                ApplyOrigin(created, origin);
                return created;
            },
            updateValueFactory: (_, existing) =>
            {
                ApplyOrigin(existing, origin);
                return existing;
            });

        return Task.FromResult((IUser)updated);
    }

    public Task UpdateUser(IUser stored, User? origin = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _users.AddOrUpdate(
            stored.TelegramId,
            addValueFactory: _ =>
            {
                var created = new InMemoryUser { TelegramId = stored.TelegramId };
                CopyFrom(created, stored);
                if (origin is not null) ApplyOrigin(created, origin);
                return created;
            },
            updateValueFactory: (_, existing) =>
            {
                CopyFrom(existing, stored);
                if (origin is not null) ApplyOrigin(existing, origin);
                return existing;
            });

        return Task.CompletedTask;
    }

    // -----------------------
    // Mapping / Copy helpers
    // -----------------------

    private static void CopyFrom(InMemoryChat target, IChat source)
    {
        target.TelegramId = source.TelegramId;
        target.Router = source.Router;

        // Clone to avoid holding onto a document that may be disposed/owned elsewhere
        target.Data = CloneJson(source.Data);

        // If source is also InMemoryChat, keep Title too
        if (source is InMemoryChat mem)
            target.Title = mem.Title;
    }

    private static void CopyFrom(InMemoryMessage target, IMessage source)
    {
        target.TelegramChatId = source.TelegramChatId;
        target.TelegramMessageId = source.TelegramMessageId;
        target.TelegramFromId = source.TelegramFromId;
        target.Date = source.Date;

        target.Router = source.Router;
        target.Data = CloneJson(source.Data);

        if (source is InMemoryMessage mem)
            target.Text = mem.Text;
    }

    private static void CopyFrom(InMemoryUser target, IUser source)
    {
        target.TelegramId = source.TelegramId;

        if (source is InMemoryUser mem)
            target.Username = mem.Username;
    }

    private static void ApplyOrigin(InMemoryChat target, Chat origin)
    {
        target.TelegramId = origin.Id;

        // Telegram.Bot.Types.Chat.Title may be null depending on chat type
        target.Title = origin.Title;
    }

    private static void ApplyOrigin(InMemoryMessage target, Message origin)
    {
        target.TelegramChatId = origin.Chat.Id;
        target.TelegramMessageId = origin.MessageId;
        target.TelegramFromId = origin.From?.Id ?? 0;
        target.Date = origin.Date;

        target.Text = origin.Text;
    }

    private static void ApplyOrigin(InMemoryUser target, User origin)
    {
        target.TelegramId = origin.Id;
        target.Username = origin.Username;
    }

    private static JsonDocument? CloneJson(JsonDocument? doc)
    {
        if (doc is null) return null;

        // Safe clone: serialize to raw text then parse.
        // This avoids referencing internal buffers owned by the original document.
        var raw = doc.RootElement.GetRawText();
        return JsonDocument.Parse(raw);
    }
}