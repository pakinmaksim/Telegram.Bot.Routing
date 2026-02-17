using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Routing.Core;

public class TelegramHostingService : IHostedService
{
    private readonly ITelegramBotClient _bot;
    private readonly TelegramRoutingSystem _system;
    private readonly ILogger<TelegramHostingService> _logger;

    public TelegramHostingService(
        ITelegramBotClient bot,
        TelegramRoutingSystem system,
        ILogger<TelegramHostingService> logger)
    {
        _bot = bot;
        _system = system;
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _bot.StartReceiving(
            UpdateHandler, PollingErrorHandler, new ReceiverOptions()
            {
                AllowedUpdates = [ UpdateType.Message, UpdateType.CallbackQuery ]
            },
            cancellationToken: cancellationToken);
        
        return Task.CompletedTask;
    }

    private async Task UpdateHandler(ITelegramBotClient _, Update update, CancellationToken ct)
    {
        await _system.HandleUpdate(update, ct);
    }

    private Task PollingErrorHandler(ITelegramBotClient _, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Error while handling update");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}