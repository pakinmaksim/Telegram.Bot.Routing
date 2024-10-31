using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Routing.Binding;
using Telegram.Bot.Routing.Contexts;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Contexts.Chats.RouteResults;
using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Contexts.Messages.RouteResults;
using Telegram.Bot.Routing.Registration;
using Telegram.Bot.Routing.Storage.Serializers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CallbackData = Telegram.Bot.Routing.Contexts.Messages.CallbackData;

namespace Telegram.Bot.Routing;

public class TelegramBotRoutingSystem
{
    public TelegramBotRoutingOptions Config { get; private set; }
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramBotRoutingSystem> _logger;

    public TelegramBotRoutingSystem(
        IServiceProvider serviceProvider,
        TelegramBotRoutingOptions config,
        ILogger<TelegramBotRoutingSystem> logger)
    {
        _serviceProvider = serviceProvider;
        Config = config;
        _logger = logger;
    }
    
    public async Task HandleUpdate(Update update, CancellationToken ct = default)
    {
        try
        { 
            await using var scope = CreateTelegramScope(update);
            var context = scope.ServiceProvider.GetRequiredService<ITelegramContext>();
            
            await (update.Type switch
            {
                UpdateType.Message => HandleMessage(context, ct),
                UpdateType.CallbackQuery => HandleCallbackQuery(context, ct),
                _ => Task.CompletedTask
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during update handling");
        }
    }

    private TTelegramContext SetupTelegramContext<TTelegramContext>(
        IServiceProvider serviceProvider, 
        Update? update = null)
        where TTelegramContext : TelegramContext
    {
        var contextController = serviceProvider.GetRequiredService<TelegramScopeManager>();
        contextController.SetCurrentContext<TTelegramContext>(update);
        return serviceProvider.GetRequiredService<TTelegramContext>();
    }

    public AsyncServiceScope CreateTelegramScope(
        Update? update = null)
    {
        var scope = _serviceProvider.CreateAsyncScope();
        SetupTelegramContext<TelegramContext>(scope.ServiceProvider, update);
        return scope;
    }
    
    public async Task<AsyncServiceScope> CreateChatScope(
        long chatId,
        ITelegramContext? telegramContext = null,
        CancellationToken ct = default)
    {
        var scope = _serviceProvider.CreateAsyncScope();
        var context = SetupTelegramContext<TelegramChatContext>(scope.ServiceProvider, telegramContext?.Update);
        await context.InitializeChat(chatId, ct);
        return scope;
    }

    public async Task<AsyncServiceScope> CreateChatScope(
        Chat chat,
        ITelegramContext? telegramContext = null,
        CancellationToken ct = default)
    {
        var scope = _serviceProvider.CreateAsyncScope();
        var context = SetupTelegramContext<TelegramChatContext>(scope.ServiceProvider, telegramContext?.Update);
        await context.InitializeChat(chat, ct);
        return scope;
    }

    public async Task<AsyncServiceScope> CreateMessageScope(
        long chatId,
        int messageId,
        ITelegramChatContext? chatContext = null,
        CancellationToken ct = default)
    {
        var scope = _serviceProvider.CreateAsyncScope();
        var context = SetupTelegramContext<TelegramMessageContext>(scope.ServiceProvider, chatContext?.Update);
        if (chatContext != null) context.InitializeChat(chatContext.Chat);
        else await context.InitializeChat(chatId, ct);
        await context.InitializeMessage(messageId, ct);
        return scope;
    }

    public async Task<AsyncServiceScope> CreateMessageScope(
        Message message,
        ITelegramChatContext? chatContext = null,
        CancellationToken ct = default)
    {
        var scope = _serviceProvider.CreateAsyncScope();
        var context = SetupTelegramContext<TelegramMessageContext>(scope.ServiceProvider, chatContext?.Update);
        if (chatContext != null) context.InitializeChat(chatContext.Chat);
        else await context.InitializeChat(message.Chat, ct);
        await context.InitializeMessage(message, ct);
        return scope;
    }

    public async Task<AsyncServiceScope> CreateNewMessageScope(
        long chatId,
        string routerName,
        object? routerData,
        CancellationToken ct = default)
    {
        var scope = _serviceProvider.CreateAsyncScope();
        var context = SetupTelegramContext<TelegramMessageContext>(scope.ServiceProvider);
        await context.InitializeChat(chatId, ct);
        await context.InitializeMessage(routerName, routerData, ct);
        return scope;
    }

    public async Task<AsyncServiceScope> CreateNewMessageScope(
        TelegramChatContext chatContext,
        string routerName,
        object? routerData,
        CancellationToken ct = default)
    {
        var scope = _serviceProvider.CreateAsyncScope();
        var context = SetupTelegramContext<TelegramMessageContext>(scope.ServiceProvider, chatContext.Update);
        context.InitializeChat(chatContext.Chat);
        await context.InitializeMessage(routerName, routerData, ct);
        return scope;
    }

    private async Task HandleMessage(ITelegramContext telegramContext, CancellationToken ct)
    {
        // Extract key values of update
        var update = telegramContext.Update!;
        var message = update.Message!;
        var chat = message.Chat;
        var user = message.From;

        // Setup current context
        await using var chatScope = await CreateChatScope(chat, telegramContext, ct);
        var context = chatScope.ServiceProvider.GetRequiredService<ITelegramChatContext>();
        
        // Reconstruct received values
        await context.ReconstructMessage(message, null, null, ct);
        if (user != null) await context.ReconstructUser(user, ct);
        
        // Call route
        await InvokeMessageRoute(context, message, ct);
        
        // Save state
        await context.SaveChat(ct);
    }

    private async Task HandleCallbackQuery(ITelegramContext telegramContext, CancellationToken ct)
    {
        // Extract key values of update
        var update = telegramContext.Update!;
        var callbackQuery = update.CallbackQuery!;
        var message = callbackQuery.Message!;
        var chat = message.Chat;
        var user = callbackQuery.From;
        
        // Setup current context
        await using var chatScope = await CreateChatScope(chat, telegramContext, ct);
        var chatContext = chatScope.ServiceProvider.GetRequiredService<TelegramChatContext>();
        await using var scope = await CreateMessageScope(message, chatContext, ct);
        var context = scope.ServiceProvider.GetRequiredService<TelegramMessageContext>();
        
        // Reconstruct received values
        await context.ReconstructUser(user, ct);
        
        // Call route
        await InvokeCallbackRoute(context, callbackQuery, ct);
        
        // Save state
        await context.SaveMessage(ct);
        await context.SaveChat(ct);
    }

    private DefinedChatRouter GetChatRouter(string routerName) =>
        Config.ChatRouters.TryGetValue(routerName, out var router) ? router : throw new ArgumentException($"Cannot find chat router with name '{routerName}'");
    private DefinedMessageRouter GetMessageRouter(string routerName) => 
        Config.MessageRouters.TryGetValue(routerName, out var router) ? router : throw new ArgumentException($"Cannot find message router with name '{routerName}'");
    
    private DefinedChatRouter GetChatRouter(Type type) => 
        Config.ChatRouters.Values.FirstOrDefault(x => x.Type == type) ?? throw new ArgumentException($"Cannot find chat router with type '{type.Name}'");
    private DefinedMessageRouter GetMessageRouter(Type type) => 
        Config.MessageRouters.Values.FirstOrDefault(x => x.Type == type) ?? throw new ArgumentException($"Cannot find message router with type '{type.Name}'");
    
    public string GetChatRouterName(Type type) => 
        GetChatRouter(type).Name ?? throw new ArgumentException($"Cannot find chat router with type '{type.Name}'");
    public string GetMessageRouterName(Type type) => 
        GetMessageRouter(type).Name ?? throw new ArgumentException($"Cannot find message router with type '{type.Name}'");

    private DefinedRoute? GetMessageRoute(string routerName, string? routeName)
    {
        var router = GetChatRouter(routerName);
        if (routeName is null) return router.DefaultMessageRoute;
        return router.MessageRoutes.TryGetValue(routeName, out var route) ? route : router.DefaultMessageRoute;
    }
    private DefinedRoute? GetCallbackRoute(string routerName, string routeName)
    {
        var router = GetMessageRouter(routerName);
        return router.CallbackRoutes.TryGetValue(routeName, out var route) ? route : router.DefaultCallbackRoute;
    }
    
    public async Task<object?> InvokeChatContextIndex(
        ITelegramChatContext context,
        CancellationToken ct = default)
    {
        if (context.Chat.RouterName is null) return null;
        var router = GetChatRouter(context.Chat.RouterName);
        if (router.IndexRoute is null) return null;
        
        var result = await InvokeRoute(
            route: router.IndexRoute,
            routerData: context.Chat.RouterData,
            serviceProvider: context.ServiceProvider,
            message: null,
            callbackQuery: null,
            ct: ct);
        if (result is IChatRouteResult chatRouteResult) 
            return await ProcessChatRouteResult(chatRouteResult, context, ct);
        return result;
    }
    
    public async Task<object?> InvokeMessageRoute(
        ITelegramChatContext context,
        Message? message,
        CancellationToken ct = default)
    {
        if (context.Chat.RouterName is null) return null;
        var route = GetMessageRoute(context.Chat.RouterName, context.Chat.RouteName);
        if (route is null) return null;
        
        var result = await InvokeRoute(
            route: route,
            routerData: context.Chat.RouterData,
            serviceProvider: context.ServiceProvider,
            message: message,
            callbackQuery: null,
            ct: ct);
        if (result is IChatRouteResult chatRouteResult) 
            return await ProcessChatRouteResult(chatRouteResult, context, ct);
        return result;
    }
    
    public async Task<object?> InvokeMessageContextIndex(
        ITelegramMessageContext context,
        CancellationToken ct = default)
    {
        if (context.Message.RouterName is null) return null;
        var router = GetMessageRouter(context.Message.RouterName);
        
        var result = await InvokeRoute(
            route: router.IndexRoute,
            routerData: context.Message.RouterData,
            serviceProvider: context.ServiceProvider,
            message: null,
            callbackQuery: null,
            ct: ct);
        if (result is IMessageRouteResult messageRouteResult) 
            return await ProcessMessageRouteResult(messageRouteResult, context, ct);
        return result;
    }
    
    public async Task<object?> InvokeCallbackRoute(
        ITelegramMessageContext context,
        CallbackQuery? callbackQuery,
        CancellationToken ct = default)
    {
        if (context.Message.RouterName is null) return null;
        var callbackData = CallbackData.Parse(callbackQuery?.Data);
        if (callbackData.Action is null) return null;
        var route = GetCallbackRoute(context.Message.RouterName, callbackData.Action);
        if (route is null) return null;
        
        var result = await InvokeRoute(
            route: route,
            routerData: context.Message.RouterData,
            serviceProvider: context.ServiceProvider,
            message: null,
            callbackQuery: callbackQuery,
            ct: ct);
        if (result is IMessageRouteResult messageRouteResult) 
            return await ProcessMessageRouteResult(messageRouteResult, context, ct);
        return result;
    }

    private async Task<object?> ProcessChatRouteResult(
        IChatRouteResult result, 
        ITelegramChatContext context,
        CancellationToken ct = default)
    {
        switch (result)
        {
            case SendMessageResult sendMessage:
                return await context.SendMessage(sendMessage.Message, ct: ct);
            case SendRouterMessageResult sendRouterMessage:
                return await context.SendRouterMessage(sendRouterMessage.RouterName, sendRouterMessage.RouterData, ct: ct);
            case Contexts.Chats.RouteResults.ChatRerouteResult chatRoute:
                context.SetChatRouter(chatRoute.RouterName, chatRoute.RouterData);
                return await InvokeChatContextIndex(context, ct);
            default: throw new NotImplementedException(result.GetType().Name);
        }
    }

    private async Task<object?> ProcessMessageRouteResult(
        IMessageRouteResult result, 
        ITelegramMessageContext context,
        CancellationToken ct = default)
    {
        switch (result)
        {
            case ShowMessageResult showMessage:
                return await context.ShowMessage(showMessage.Message, ct);
            case SetKeyboardResult setKeyboard:
                return await context.SetKeyboard(setKeyboard.Keyboard, ct);
            case MessageRerouteResult changeMessageRouter:
                context.SetMessageRouter(changeMessageRouter.RouterName, changeMessageRouter.RouterData);
                return await InvokeMessageContextIndex(context, ct);
            case Contexts.Messages.RouteResults.ChatRerouteResult chatRoute:
                context.SetChatRouter(chatRoute.RouterName, chatRoute.RouterData);
                return await InvokeChatContextIndex(context, ct);
            default: throw new NotImplementedException(result.GetType().Name);
        }
    }
    
    private async Task<object?> InvokeRoute(
        DefinedRoute route,
        object? routerData,
        IServiceProvider serviceProvider,
        Message? message,
        CallbackQuery? callbackQuery,
        CancellationToken ct = default)
    {
        var router = route.Method.IsStatic || route.Method.DeclaringType is null ? null 
            : serviceProvider.GetRequiredService(route.Method.DeclaringType);
        var parameters = ActivateRouteParameters(
            parameters: route.Parameters,
            routerData: routerData,
            serviceProvider: serviceProvider,
            message: message,
            callbackQuery: callbackQuery,
            ct: ct);
        var result = route.Method.Invoke(router, parameters.ToArray());
        if (result is not Task task) return result;
        await task;
        
        var taskType = task.GetType();
        if (!taskType.IsGenericType || taskType.GetGenericTypeDefinition() != typeof(Task<>))
            return null;
        
        var resultProperty = taskType.GetProperty("Result");
        return resultProperty?.GetValue(task);
    }

    private IEnumerable<object?> ActivateRouteParameters(
        DefinedRouteParameter[] parameters,
        object? routerData,
        IServiceProvider serviceProvider,
        Message? message,
        CallbackQuery? callbackQuery,
        CancellationToken? ct)
    {
        return parameters.Select(parameter => parameter.Kind switch
        {
            RouteParameterKind.Service => serviceProvider.GetRequiredService(parameter.Type!),
            RouteParameterKind.RouterData => ActivateRouterData(routerData, parameter, serviceProvider),
            RouteParameterKind.UserMessage => message ?? parameter.DefaultValue,
            RouteParameterKind.CallbackQuery => callbackQuery ?? parameter.DefaultValue,
            RouteParameterKind.CancellationToken => ct ?? parameter.DefaultValue,
            _ => parameter.DefaultValue
        });
    }
    
    private object? ActivateRouterData(
        object? routerData, 
        DefinedRouteParameter parameter,
        IServiceProvider serviceProvider)
    {
        if (parameter.Kind != RouteParameterKind.RouterData) return parameter.DefaultValue;
        if (routerData is null) return parameter.DefaultValue;
        if (parameter.Type is null) return parameter.DefaultValue;

        var actualType = routerData.GetType();
        if (parameter.Type.IsAssignableFrom(actualType))
            return routerData;

        if (routerData is string stringRouteData)
        {
            var serializer = serviceProvider.GetRequiredService<IRouteDataSerializer>();
            return serializer.Deserialize(stringRouteData, parameter.Type) ?? parameter.DefaultValue;
        }

        return parameter.DefaultValue;
    }
}