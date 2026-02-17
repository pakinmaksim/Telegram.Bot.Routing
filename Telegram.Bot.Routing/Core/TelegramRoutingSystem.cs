using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Routing.Contexts;
using Telegram.Bot.Routing.Contexts.BotMessages;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Registration;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Routing.Core;

public class TelegramRoutingSystem
{
    public TelegramRoutingConfig Config { get; set; }
    public ITelegramBotClient Bot { get; set; }
    
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramRoutingSystem> _logger;

    public TelegramRoutingSystem(
        TelegramRoutingConfig config,
        ITelegramBotClient bot,
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramRoutingSystem> logger)
    {
        Config = config;
        Bot = bot;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleUpdate(Update update, CancellationToken ct = default)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TelegramUpdateId"] = update.Id
        });
        await using var scope = SetupTelegramScope(update);
        try
        {
            await (update.Type switch
            {
                UpdateType.Message => HandleMessage(scope, ct),
                UpdateType.CallbackQuery => HandleCallbackQuery(scope, ct),
                _ => Task.CompletedTask
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during update handling");
        }
    }
    

    public TelegramScope SetupTelegramScope(Update? update = null)
    {
        var updateScope = _scopeFactory.CreateAsyncScope();
        var telegramScope = new TelegramScope(updateScope);
        telegramScope.Update = update;
        return telegramScope;
    }
    
    private async Task HandleMessage(TelegramScope scope, CancellationToken ct = default)
    {
        // Setup current context
        var context = new UserMessageContext { System = this, Scope = scope };
        await context.InitializeFromUpdate(ct);
        if (context.ChatModel.Router is null) return;
        
        // Call route
        var router = context.GetCurrentChatRouter();
        if (router is null) return;
        await router.OnUserMessage(context, ct);
        
        // Save state
        await context.Store(ct);
    }

    private async Task HandleCallbackQuery(TelegramScope scope, CancellationToken ct = default)
    {
        // Setup current context
        var context = new CallbackQueryContext { System = this, Scope = scope };
        await context.InitializeFromUpdate(ct);
        if (context.BotMessageModel.Router is null) return;

        // Call route
        if (GetDefinedBotMessageRouter(context.BotMessageModel.Router) is { } definedBotMessageRouter)
        {
            var router = context.Scope.GetBotMessageRouter(definedBotMessageRouter.Name);
            if (router is null) return;
            await router.OnCallbackQuery(context, ct);
        }
        else if (GetDefinedChatRouter(context.BotMessageModel.Router) is { } definedChatRouter)
        {
            var router = context.Scope.GetChatRouter(definedChatRouter.Name);
            if (router is null) return;
            await router.OnCallbackQuery(context, ct);
        }
        
        // Save state
        await context.Store(ct);
    }

    private DefinedChatRouter? GetDefinedChatRouter(string? routerName)
    {
        if (routerName == null) return null;

        if (!Config.ChatRouters.TryGetValue(routerName, out var router))
        {
            router = Config.ChatRouters.Values
                .FirstOrDefault(x => x.LegacyNames.Contains(routerName));
        }
        return router;
    }
    private DefinedChatRouter? GetDefinedChatRouter(Type type) 
        => Config.ChatRouters.Values.FirstOrDefault(x => x.Type == type);

    private DefinedBotMessageRouter? GetDefinedBotMessageRouter(string? routerName)
    {
        if (routerName == null) return null;

        if (!Config.BotMessageRouters.TryGetValue(routerName, out var router))
        {
            router = Config.BotMessageRouters.Values
                .FirstOrDefault(x => x.LegacyNames.Contains(routerName));
        }
        return router;
    }
    private DefinedBotMessageRouter? GetDefinedBotMessageRouter(Type type) 
        => Config.BotMessageRouters.Values.FirstOrDefault(x => x.Type == type);

    public string? GetChatRouterName<TRouter>() where TRouter : ChatRouter 
        => GetChatRouterName(typeof(TRouter));
    public string? GetChatRouterName(Type type) 
        => GetDefinedChatRouter(type)?.Name;
    public string? GetChatRouterName(string? name) 
        => GetDefinedChatRouter(name)?.Name;
    public string? GetBotMessageRouterName<TRouter>() where TRouter : BotMessageRouter 
        => GetBotMessageRouterName(typeof(TRouter));
    public string? GetBotMessageRouterName(Type type) 
        => GetDefinedBotMessageRouter(type)?.Name;
    public string? GetBotMessageRouterName(string? name) 
        => GetDefinedBotMessageRouter(name)?.Name;
}