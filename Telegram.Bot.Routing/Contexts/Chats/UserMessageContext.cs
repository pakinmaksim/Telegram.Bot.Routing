using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Contexts.Chats;

public class UserMessageContext : ChatContext
{
    public IUser? UserModel { get; internal set; } = null!;
    public Message UserMessage => Update!.Message!;
    
    
    internal async Task InitializeFromUpdate(CancellationToken ct = default)
    {
        await InitializeChat(UserMessage.Chat, ct);
        await GetMessageModel(UserMessage, ct);
        if (UserMessage.From != null) UserModel = await GetUserModel(UserMessage.From, ct);
    }
    
    internal override async Task Store(CancellationToken ct = default)
    {
        await base.Store(ct);
        if (UserModel != null) await Scope.UpdateUser(UserModel, null, ct);
    }

    public async Task DeleteUserMessage(CancellationToken ct = default)
    {
        await DeleteMessage(
            messageId: UserMessage.MessageId,
            ct: ct);
    }
}