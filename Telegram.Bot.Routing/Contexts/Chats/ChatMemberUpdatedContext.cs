using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Contexts.Chats;

public class ChatMemberUpdatedContext : ChatContext
{
    public IUser UserModel { get; internal set; } = null!;
    public ChatMemberUpdated ChatMember => Update!.ChatMember!;
    
    
    internal async Task InitializeFromUpdate(CancellationToken ct = default)
    {
        await InitializeChat(ChatMember.Chat, ct);
        UserModel = await GetUserModel(ChatMember.From, ct);
    }
    
    internal override async Task Store(CancellationToken ct = default)
    {
        await base.Store(ct);
        await Scope.UpdateUser(UserModel, null, ct);
    }
}