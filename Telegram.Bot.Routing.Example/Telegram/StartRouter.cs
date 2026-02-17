using System.Globalization;
using Telegram.Bot.Routing.Contexts.BotMessages;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Core.Messages;
using Telegram.Bot.Routing.Registration;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Routing.Storage.InMemory.Models;
using Telegram.Bot.Routing.Utils;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram;

[ChatRouter("chat_start", isDefault: true)]
public class StartRouter : ChatRouter
{
    public override async Task OnRouteIn(ChatContext context, CancellationToken ct = default)
    {
        await context.ShowMessage(
            text: "Hello there! What do you want to do?",
            replyMarkup: new InlineKeyboardMarkup(
            [
                [Button("Get weather", "weather")],
                [Button("Test alerts", "alerts")],
                [Button("About us", "about")],
            ]),
            ct: ct);
    }

    private async Task OnWeatherButtonClicked(CallbackQueryContext context, CancellationToken ct = default)
    {
        await context.RouteChatTo<WeatherRouter>(ct);
    }
    private async Task OnAlertsButtonClicked(CallbackQueryContext context, CancellationToken ct = default)
    {
        await context.AnswerCallbackQuery("Test succeed", false, ct);
    }
    private async Task OnAboutButtonClicked(CallbackQueryContext context, CancellationToken ct = default)
    {
        await context.ShowMessage(
            text: "Maksim Pakin, no more",
            replyMarkup: new InlineKeyboardMarkup(
            [
                [Button("Back", "menu")],
            ]),
            ct: ct);
    }
    private async Task OnMenuButtonClicked(CallbackQueryContext context, CancellationToken ct = default)
    {
        await OnRouteIn(context, ct);
    }

    public override async Task OnUserMessage(UserMessageContext context, CancellationToken ct = default)
    {
        if (context.UserMessage.Text?.StartsWith("/start") == true)
            await OnRouteIn(context, ct);
        else
        {
            var message = await context.SendMessage("Use buttons please", ct: ct);
            await context.DeleteUserMessage(ct);
            await Task.Delay(3000, ct);
            await context.DeleteMessage(message.TelegramMessageId, ct: ct);
        }
    }

    public override Task OnCallbackQuery(CallbackQueryContext context, CancellationToken ct = default)
    {
        var callback = context.CallbackData;
        return callback.Action switch
        {
            "weather" => OnWeatherButtonClicked(context, ct),
            "about" => OnAboutButtonClicked(context, ct),
            "alerts" => OnAlertsButtonClicked(context, ct),
            "menu" => OnMenuButtonClicked(context, ct),
            _ => Task.CompletedTask
        };
    }
}