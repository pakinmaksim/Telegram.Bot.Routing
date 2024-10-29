using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Routing.Example.Hosting;

public class TelegramHostingService : IHostedService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly TelegramBotRoutingSystem _telegramBotRoutingSystem;

    public TelegramHostingService(
        ITelegramBotClient telegramBotClient,
        TelegramBotRoutingSystem telegramBotRoutingSystem)
    {
        _telegramBotClient = telegramBotClient;
        _telegramBotRoutingSystem = telegramBotRoutingSystem;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _telegramBotClient.StartReceiving(
            UpdateHandler, PollingErrorHandler, new ReceiverOptions()
            {
                AllowedUpdates = [ UpdateType.Message, UpdateType.CallbackQuery ]
            },
            cancellationToken: cancellationToken);
        
        return Task.CompletedTask;
    }

    private async Task UpdateHandler(ITelegramBotClient _, Update update, CancellationToken ct)
    {
        await _telegramBotRoutingSystem.HandleUpdate(update, ct);
    }

    private Task PollingErrorHandler(ITelegramBotClient _, Exception update, CancellationToken ct)
    {
        Console.WriteLine(update);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}