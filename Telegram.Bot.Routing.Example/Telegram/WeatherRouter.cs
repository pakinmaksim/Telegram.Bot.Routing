using Telegram.Bot.Routing.Contexts.BotMessages;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Core.Messages;
using Telegram.Bot.Routing.Registration;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Example.Telegram;

public class WeatherForm
{
    public string CurrentStep { get; set; } = null!;
    public string? Query { get; set; }
    public DateOnly? Date { get; set; }
}

[ChatRouter("chat_weather")]
public class WeatherRouter : ChatRouter<WeatherForm>
{
    public override async Task OnRouteIn(ChatContext context, WeatherForm data, CancellationToken ct = default)
    {
        await AskQuery(context, data, ct);
    }
    
    private async Task AskQuery(ChatContext context, WeatherForm data, CancellationToken ct = default)
    {
        data.CurrentStep = "enter_query";
        data.Query = null;
        
        await context.ShowMessage(new ShowBotMessage()
        {
            Text = "Enter search query",
            ReplyMarkup = new InlineKeyboardMarkup(
            [
                [
                    Button("USA", "set_query", x => x.SetValue("t", "USA")),
                    Button("RU", "set_query", x => x.SetValue("t", "Russia")),
                    Button("CN", "set_query", x => x.SetValue("t", "China"))
                ],
                [Button("Cancel", "menu")],
            ])
        }, ct: ct);
    }
    
    private async Task OnQueryMessageReceived(UserMessageContext context, WeatherForm data, CancellationToken ct = default)
    {
        var query = context.UserMessage.Text?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            await context.SendMessage(new NewBotMessage()
            {
                Text = "Incorrect message"
            }, ct);
            return;
        }
        data.Query = query;
        await AskDate(context, data, ct);
    }


    private async Task OnQueryButtonClicked(CallbackQueryContext context, WeatherForm data, CancellationToken ct = default)
    {
        var callback = context.CallbackData;
        if (!callback.TryGetString("t", out var query)) return;
        data.Query = query;
        await AskDate(context, data, ct);
    }
    
    private async Task AskDate(ChatContext context, WeatherForm data, CancellationToken ct = default)
    {
        data.CurrentStep = "enter_date";
        data.Date = null;
        
        await context.ShowMessage(new ShowBotMessage()
        {
            Text = "Enter or select date",
            ReplyMarkup = new InlineKeyboardMarkup(
            [
                [
                    Button("Yesterday", "set_date", x => x.SetValue("d", -1)),
                    Button("Today", "set_date", x => x.SetValue("d", 0)),
                    Button("Tomorrow", "set_date", x => x.SetValue("d", 1))
                ],
                [Button("Cancel", "menu")],
            ])
        }, ct: ct);
    }
    
    private async Task OnDateMessageReceived(UserMessageContext context, WeatherForm data, CancellationToken ct = default)
    {
        var input = context.UserMessage.Text?.Trim();
        if (DateOnly.TryParseExact(input, "dd.MM.yyy", out var date))
        {
            await context.SendMessage(new NewBotMessage()
            {
                Text = "Incorrect date format"
            }, ct);
            return;
        }
        data.Date = date;
        await ShowResult(context, data, ct);
    }


    private async Task OnDateButtonClicked(CallbackQueryContext context, WeatherForm data, CancellationToken ct = default)
    {
        var callback = context.CallbackData;
        if (!callback.TryGetInt("d", out var deltaDays)) return;
        var date = DateTime.Now.AddDays(deltaDays);
        data.Date = DateOnly.FromDateTime(date);
        await ShowResult(context, data, ct);
    }
    
    private async Task ShowResult(ChatContext context, WeatherForm data, CancellationToken ct = default)
    {
        data.CurrentStep = "show_result";

        var dataHash = $"{data.Query}{data.Date}".GetHashCode();
        var temp = new Random(dataHash).Next(-10, 10);
        await context.ShowMessage(new ShowBotMessage()
        {
            Text = $"Search: {data.Query}\n" +
                   $"Date: {data.Date}\n" +
                   $"Temp: {(temp > 0 ? "+" + temp : temp)} Celsius",
            ReplyMarkup = new InlineKeyboardMarkup(
            [
                [Button("Back", "menu")],
            ])
        }, ct: ct);
    }
    
    public override Task OnUserMessage(UserMessageContext context, WeatherForm data, CancellationToken ct = default)
    {
        return data.CurrentStep switch
        {
            "enter_query" => OnQueryMessageReceived(context, data, ct),
            "enter_date" => OnDateMessageReceived(context, data, ct),
            "show_result" => ShowResult(context, data, ct),
            _ => Task.CompletedTask
        };
    }

    public override Task OnCallbackQuery(CallbackQueryContext context, WeatherForm data, CancellationToken ct = default)
    {
        var callback = context.CallbackData;
        return callback.Action switch
        {
            "set_query" => OnQueryButtonClicked(context, data, ct),
            "set_date" => OnDateButtonClicked(context, data, ct),
            "menu" => context.RouteChatTo<StartRouter>(ct),
            _ => Task.CompletedTask
        };
    }
}