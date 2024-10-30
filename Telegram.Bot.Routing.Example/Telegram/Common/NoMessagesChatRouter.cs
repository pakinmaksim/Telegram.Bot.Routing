using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Contexts.Messages;

namespace Telegram.Bot.Routing.Example.Telegram.Common;

[ChatRouter("no_messages")]
public class NoMessagesChatRouter : ChatRouter
{
    [MessageRoute("default", isDefault: true)]
    public async Task Default(CancellationToken ct)
    {
        var data = Context.GetChatRouterData<NoMessagesData>();
        if (data is null) return;
        
        await Context.SendMessage("Use buttons pls", ct: ct);
        await Context.SendRouterMessage(data.RepeatMessageRouterName, data.RepeatMessageRouterData, ct);
    }
}

public class NoMessagesData
{
    public required string RepeatMessageRouterName { get; init; }
    public string? RepeatMessageRouterData { get; init; }
}

public static class TelegramMessageContextExtensions
{
    public static void ButtonPressRequired(this TelegramMessageContext context)
    {
        if (context.Message.RouterName is null) return;
        var data = new NoMessagesData()
        {
            RepeatMessageRouterName = context.Message.RouterName,
            RepeatMessageRouterData = context.Message.RouterData,
        };
        context.SetChatRouter<NoMessagesChatRouter>(data);
    }
}