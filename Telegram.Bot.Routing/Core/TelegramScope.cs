using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Contexts;
using Telegram.Bot.Routing.Contexts.BotMessages;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Core;

public class TelegramScope : IAsyncDisposable
{
    private readonly ConcurrentDictionary<long, IUser?> _loadedUsers = new();
    private readonly ConcurrentDictionary<long, IChat?> _loadedChats = new();
    private readonly ConcurrentDictionary<(long, int), IMessage?> _loadedMessages = new();
    
    private readonly AsyncServiceScope _asyncScope;
    private readonly TelegramRoutingSystem _system;
    private readonly ITelegramStorage _storage;

    public Update? Update;
    
    public TelegramScope(AsyncServiceScope asyncScope)
    {
        _asyncScope = asyncScope;
        _system = asyncScope.ServiceProvider.GetRequiredService<TelegramRoutingSystem>();
        _storage = asyncScope.ServiceProvider.GetRequiredService<ITelegramStorage>();
    }

    public async Task<IChat?> GetChat(long chatId, CancellationToken ct = default)
    {
        if (_loadedChats.TryGetValue(chatId, out var model))
            return model;

        model = await _storage.GetChat(chatId, ct);
        _loadedChats[chatId] = model;
        return model;
    }

    public async Task<IChat> UpsertChat(Chat origin, string? router, JsonDocument? data, CancellationToken ct = default)
    {
        var model = await _storage.UpsertChat(origin, router, data, ct);
        _loadedChats[model.TelegramId] = model;
        return model;
    }

    public async Task UpdateChat(IChat stored, Chat? origin = null, CancellationToken ct = default)
    {
        await _storage.UpdateChat(stored, origin, ct);
        _loadedChats[stored.TelegramId] = stored;
    }

    public async Task<IMessage?> GetMessage(long chatId, int messageId, CancellationToken ct = default)
    {
        var key = (chatId, messageId);
        if (_loadedMessages.TryGetValue(key, out var model))
            return model;

        model = await _storage.GetMessage(chatId, messageId, ct);
        _loadedMessages[key] = model;
        return model;
    }
    public async Task<IMessage> GetMessage(Message origin, CancellationToken ct = default)
    {
        var chatId = origin.Chat.Id;
        var messageId = origin.MessageId;
        var stored = await GetMessage(chatId, messageId, ct);
        if (stored is null)
        {
            var router = origin.From?.Id == _system.Bot.BotId ? _system.Config.DefaultBotMessageRouterName : null; 
            stored = await UpsertMessage(origin, router, null, ct);
        }
        else await UpdateMessage(stored, origin, ct);
        return stored;
    }

    public async Task<IMessage> UpsertMessage(Message origin, string? router, JsonDocument? data, CancellationToken ct = default)
    {
        var model = await _storage.UpsertMessage(origin, router, data, ct);
        var key = (model.TelegramChatId, model.TelegramMessageId);
        _loadedMessages[key] = model;
        return model;
    }

    public async Task UpdateMessage(IMessage stored, Message? origin = null, CancellationToken ct = default)
    {
        await _storage.UpdateMessage(stored, origin, ct);
        var key = (stored.TelegramChatId, stored.TelegramMessageId);
        _loadedMessages[key] = stored;
    }

    public async Task<IUser?> GetUser(long userId, CancellationToken ct = default)
    {
        if (_loadedUsers.TryGetValue(userId, out var model))
            return model;

        model = await _storage.GetUser(userId, ct);
        _loadedUsers[userId] = model;
        return model;
    }

    public async Task<IUser> UpsertUser(User origin, CancellationToken ct = default)
    {
        var model = await _storage.UpsertUser(origin, ct);
        _loadedUsers[model.TelegramId] = model;
        return model;
    }

    public async Task UpdateUser(IUser stored, User? origin = null, CancellationToken ct = default)
    {
        await _storage.UpdateUser(stored, origin, ct);
        _loadedUsers[stored.TelegramId] = stored;
    }
    
    public TRouter GetChatRouter<TRouter>(CancellationToken ct = default)
        where TRouter : ChatRouter
    {
        var name = _system.GetChatRouterName<TRouter>();
        var router = GetChatRouter(name);
        return (router as TRouter)!;
    }
    public ChatRouter GetChatRouter(Type type)
    {
        var name = _system.GetChatRouterName(type);
        return GetChatRouter(name)!;
    }
    public ChatRouter? GetChatRouter(string? name)
    {
        if (name is null) return null;
        return _asyncScope.ServiceProvider.GetRequiredKeyedService<ChatRouter>(name);
    }
    public TRouter GetBotMessageRouter<TRouter>(CancellationToken ct = default)
        where TRouter : BotMessageRouter
    {
        var name = _system.GetBotMessageRouterName<TRouter>();
        var router = GetBotMessageRouter(name);
        return (router as TRouter)!;
    }
    public BotMessageRouter GetBotMessageRouter(Type type)
    {
        var name = _system.GetBotMessageRouterName(type);
        return GetBotMessageRouter(name)!;
    }
    public BotMessageRouter? GetBotMessageRouter(string? name)
    {
        if (name is null) return null;
        return _asyncScope.ServiceProvider.GetRequiredKeyedService<BotMessageRouter>(name);
    }

    public async Task<ChatContext> GetChatContext(long chatId, CancellationToken ct = default)
    {
        var context = new ChatContext { System = _system, Scope = this };
        await context.InitializeChat(chatId, ct);
        return context;
    }
    
    public async Task<BotMessageContext> CreateBotMessageContext(long chatId, int messageId, CancellationToken ct = default)
    {
        var context = new BotMessageContext { System = _system, Scope = this };
        await context.InitializeChat(chatId, ct);
        await context.InitializeMessage(messageId, ct);
        return context;
    }
    
    public async Task<BotMessageContext> CreateNewBotMessageContext(
        long chatId,
        string router,
        object? data,
        CancellationToken ct = default)
    {
        var context = new BotMessageContext { System = _system, Scope = this };
        await context.InitializeChat(chatId, ct);
        context.InitializeUnsentMessage(router, data);
        return context;
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _asyncScope.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}