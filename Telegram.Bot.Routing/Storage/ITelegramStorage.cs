using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Storage;

public interface ITelegramStorage
{
    public Task<IUser> ConstructUser(User original, CancellationToken ct = default);
    public Task<IUser?> GetUser(long userId, CancellationToken ct = default);
    public Task SetUser(IUser user, CancellationToken ct = default);
    
    public Task<IChat> ConstructChat(Chat original, CancellationToken ct = default);
    public Task<IChat?> GetChat(long chatId, CancellationToken ct = default);
    public Task SetChat(IChat chat, CancellationToken ct = default);

    public Task<IMessage> ConstructMessage(Message original, CancellationToken ct = default);
    public Task<IMessage> ConstructMessage(long chatId, int messageId, string routerName, string? routerData, CancellationToken ct = default);
    public Task<IMessage?> GetMessage(long chatId, int messageId, CancellationToken ct = default);
    public Task SetMessage(IMessage message, CancellationToken ct = default);
}