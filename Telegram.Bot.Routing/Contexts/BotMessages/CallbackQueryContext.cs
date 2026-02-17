using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Routing.Utils;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Contexts.BotMessages;

public class CallbackQueryContext : BotMessageContext
{
    public IUser? UserModel { get; internal set; } = null!;
    public CallbackQuery CallbackQuery => Update!.CallbackQuery!;
    public CallbackData CallbackData => CallbackData.Parse(CallbackQuery.Data);
    
    
    internal async Task InitializeFromUpdate(CancellationToken ct = default)
    {
        await InitializeChat(CallbackQuery.Message!.Chat, ct);
        await InitializeMessage(CallbackQuery.Message, ct);
        UserModel = await GetUserModel(CallbackQuery.From, ct);
    }
    
    internal override async Task Store(CancellationToken ct = default)
    {
        await base.Store(ct);
        if (UserModel != null) await Scope.UpdateUser(UserModel, null, ct);
    }
    
    public async Task AnswerCallbackQuery(
        string? text = null,
        bool showAlert = false,
        CancellationToken ct = default)
    {
        await System.Bot.AnswerCallbackQuery(
            callbackQueryId: CallbackQuery.Id,
            text: text,
            showAlert: showAlert,
            cancellationToken: ct);
    }
}