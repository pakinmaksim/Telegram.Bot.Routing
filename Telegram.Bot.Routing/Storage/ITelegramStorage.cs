using System.Text.Json;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Storage;

public interface ITelegramStorage
{
    public Task<IChat?> GetChat(long chatId, CancellationToken ct = default);
    public Task<IChat> UpsertChat(Chat origin, string? router, JsonDocument? data, CancellationToken ct = default);
    public Task UpdateChat(IChat stored, Chat? origin = null, CancellationToken ct = default);
    
    public Task<IMessage?> GetMessage(long chatId, int messageId, CancellationToken ct = default);
    public Task<IMessage> UpsertMessage(Message origin, string? router, JsonDocument? data, CancellationToken ct = default);
    public Task UpdateMessage(IMessage stored, Message? origin = null, CancellationToken ct = default);
    
    Task<IUser?> GetUser(long userId, CancellationToken ct = default);
    Task<IUser> UpsertUser(User origin, CancellationToken ct = default);
    Task UpdateUser(IUser stored, User? origin = null, CancellationToken ct = default);
}