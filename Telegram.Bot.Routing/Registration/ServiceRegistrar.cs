using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Binding;
using Telegram.Bot.Routing.Contexts;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Contexts.Messages;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.Serializers;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Registration;

public static class ServiceRegistrar
{
    public static IServiceCollection AddRouteClasses(
        IServiceCollection services,
        TelegramBotRoutingOptions options)
    {
        options.ChatRouters.Clear();
        options.MessageRouters.Clear();

        var possibleTypes = options.AssembliesToRegister
            .SelectMany(x => x.DefinedTypes.Where(y => y is {IsClass: true, IsAbstract: false}));
        foreach (var type in possibleTypes)
        {
            if (TryCreateChatRouter(type, out var chatRouter, out var isDefault))
            {
                options.ChatRouters.Add(chatRouter.Name, chatRouter);
                if (isDefault) options.DefaultChatRouterName = chatRouter.Name;
                services.AddScoped(chatRouter.Type, ResolveChatRouterMethod(chatRouter.Type));
            }
            else if (TryCreateMessageRouter(type, out var messageRouter, out isDefault))
            {
                options.MessageRouters.Add(messageRouter.Name, messageRouter);
                if (isDefault) options.DefaultMessageRouterName = messageRouter.Name;
                services.AddScoped(messageRouter.Type, ResolveMessageRouterMethod(messageRouter.Type));
            }
        }

        return services;
    }

    public static IServiceCollection AddRequiredServices(
        IServiceCollection services,
        TelegramBotRoutingOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped(typeof(ITelegramStorage), options.StorageType);
        services.AddScoped(typeof(IRouteDataSerializer), options.RouteDataSerializerType);
        
        services.AddScoped<TelegramScopeManager>();
        
        services.AddScoped<TelegramContext>();
        services.Add(TelegramContextInterfaceDescriptor<ITelegramContext, TelegramContext>());
        services.AddScoped<TelegramChatContext>();
        services.Add(TelegramContextInterfaceDescriptor<ITelegramChatContext, TelegramChatContext>());
        services.AddScoped<TelegramMessageContext>();
        services.Add(TelegramContextInterfaceDescriptor<ITelegramMessageContext, TelegramMessageContext>());
        
        services.AddSingleton<TelegramBotRoutingSystem>();

        return services;
    }

    private static bool TryCreateChatRouter(TypeInfo type, out DefinedChatRouter definedRouter, out bool isDefault)
    {
        var chatRouterAttr = type.GetCustomAttribute<ChatRouterAttribute>();
        if (chatRouterAttr is null)
        {
            definedRouter = null!;
            isDefault = false;
            return false;
        }
        
        DefinedRoute? indexRoute = null;
        DefinedRoute? defaultRoute = null;
        var routes = new Dictionary<string, DefinedRoute>();
        foreach (var method in type.GetMethods().Where(x => x is {IsStatic: false}))
        {
            var messageRouteAttr = method.GetCustomAttribute<MessageRouteAttribute>();

            DefinedRoute? definedRoute = null;
            if (method.Name == "Index")
            {
                if (indexRoute != null) throw ExceptionHelper.MultipleChatRouterIndex(type);
                
                definedRoute ??= CreateRoute(method);
                indexRoute = definedRoute;
            }
            if (messageRouteAttr is null) continue;
            
            definedRoute ??= CreateRoute(method);
            routes.Add(messageRouteAttr.Name, definedRoute);

            if (messageRouteAttr.IsDefault)
            {
                if (defaultRoute != null) throw ExceptionHelper.MultipleDefaultMessageRoutes(type);
                defaultRoute = definedRoute;
            }
        }
        
        definedRouter = new DefinedChatRouter
        {
            Name = chatRouterAttr.Name,
            Type = type,
            IndexRoute = indexRoute,
            DefaultMessageRoute = defaultRoute,
            MessageRoutes = routes
        };
        if (indexRoute != null) indexRoute.RouterType = definedRouter.Type;
        if (defaultRoute != null) defaultRoute.RouterType = definedRouter.Type;
        foreach (var route in definedRouter.MessageRoutes.Values) route.RouterType = definedRouter.Type;
        isDefault = chatRouterAttr.IsDefault;
        return true;
    }

    private static bool TryCreateMessageRouter(TypeInfo type, out DefinedMessageRouter definedRouter, out bool isDefault)
    {
        var messageRouterAttr = type.GetCustomAttribute<MessageRouterAttribute>();
        if (messageRouterAttr is null)
        {
            definedRouter = null!;
            isDefault = false;
            return false;
        }
        
        DefinedRoute? indexRoute = null;
        DefinedRoute? defaultRoute = null;
        var routes = new Dictionary<string, DefinedRoute>();
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var callbackRouteAttr = method.GetCustomAttribute<CallbackRouteAttribute>();

            DefinedRoute? definedRoute = null;
            if (method.Name == "Index")
            {
                if (indexRoute != null) throw ExceptionHelper.MultipleMessageRouterIndex(type);
                
                definedRoute ??= CreateRoute(method);
                indexRoute = definedRoute;
            }
            if (callbackRouteAttr is null) continue;
            
            definedRoute ??= CreateRoute(method);
            routes.Add(callbackRouteAttr.Name, definedRoute);

            if (callbackRouteAttr.IsDefault)
            {
                if (defaultRoute != null) throw ExceptionHelper.MultipleDefaultCallbackRoutes(type);
                defaultRoute = definedRoute;
            }
        }
        if (indexRoute is null) throw ExceptionHelper.MessageRouterIndexMissing(type);
        
        definedRouter = new DefinedMessageRouter
        {
            Name = messageRouterAttr.Name,
            Type = type,
            IndexRoute = indexRoute,
            DefaultCallbackRoute = defaultRoute,
            CallbackRoutes = routes
        };
        indexRoute.RouterType = definedRouter.Type;
        if (defaultRoute != null) defaultRoute.RouterType = definedRouter.Type;
        foreach (var route in definedRouter.CallbackRoutes.Values) route.RouterType = definedRouter.Type;
        isDefault = messageRouterAttr.IsDefault;
        return true;
    }

    private static bool IsReturn<TType>(MethodInfo method)
    {
        var returnType = method.ReturnType;
        var expectedType = typeof(TType);
        if (expectedType.IsAssignableFrom(returnType)) return true;
        
        if (typeof(Task).IsAssignableFrom(returnType) && returnType.IsGenericType)
        {
            var genericArgument = returnType.GetGenericArguments()[0];
            if (expectedType.IsAssignableFrom(genericArgument)) return true;
        }
        
        return false;
    }

    private static DefinedRoute CreateRoute(MethodInfo method)
    {
        return new DefinedRoute()
        {
            Method = method,
            Parameters = GetRouteParameters(method).ToArray()
        };
    }
    
    private static IEnumerable<DefinedRouteParameter> GetRouteParameters(MethodInfo method)
    {
        foreach (var parameter in method.GetParameters())
        {
            if (parameter.GetCustomAttribute<FromServicesAttribute>() is not null)
            {
                yield return new DefinedRouteParameter()
                {
                    Kind = RouteParameterKind.Service,
                    Type = parameter.ParameterType,
                    DefaultValue = parameter.HasDefaultValue ? Type.Missing : null
                };
                continue;
            }
            
            if (parameter.GetCustomAttribute<FromRouterDataAttribute>() is not null)
            {
                yield return new DefinedRouteParameter()
                {
                    Kind = RouteParameterKind.RouterData,
                    Type = parameter.ParameterType,
                    DefaultValue = parameter.HasDefaultValue ? Type.Missing : null
                };
                continue;
            }

            if (parameter.ParameterType == typeof(Message))
            {
                yield return new DefinedRouteParameter()
                {
                    Kind = RouteParameterKind.UserMessage,
                    DefaultValue = parameter.HasDefaultValue ? Type.Missing : null
                };
                continue;
            }

            if (parameter.ParameterType == typeof(CallbackQuery))
            {
                yield return new DefinedRouteParameter()
                {
                    Kind = RouteParameterKind.CallbackQuery,
                    DefaultValue = parameter.HasDefaultValue ? Type.Missing : null
                };
                continue;
            }

            if (parameter.ParameterType == typeof(CallbackData))
            {
                yield return new DefinedRouteParameter()
                {
                    Kind = RouteParameterKind.CallbackData,
                    DefaultValue = parameter.HasDefaultValue ? Type.Missing : null
                };
                continue;
            }

            if (parameter.ParameterType == typeof(CancellationToken))
            {
                yield return new DefinedRouteParameter()
                {
                    Kind = RouteParameterKind.CancellationToken,
                    DefaultValue = parameter.HasDefaultValue ? Type.Missing : null
                };
                continue;
            }
            
            throw new ArgumentException(
                $"Unknown route parameter '{parameter.ParameterType.Name} {parameter.Name}'. " +
                $"See documentation for details.");
        }
    }
    
    private static ServiceDescriptor TelegramContextInterfaceDescriptor<TInterface, TService>()
        where TInterface : ITelegramContext
        where TService : TelegramContext
    {
        return new ServiceDescriptor(
            serviceType: typeof(TInterface), 
            factory: x =>
            {
                var contextController = x.GetRequiredService<TelegramScopeManager>();
                if (x.GetRequiredService(contextController.ContextType) is not TService context) 
                    throw new InvalidOperationException($"Cannot resolve service for type {typeof(TInterface).Name} " +
                                                        $"because current context for Telegram scope is {contextController.ContextType.Name}");
                context.Update = contextController.Update;
                return context;
            }, 
            lifetime: ServiceLifetime.Scoped);
    }

    private static Func<IServiceProvider, object> ResolveChatRouterMethod(Type chatRouterType)
    {
        return (serviceProvider) =>
        {
            var router = (ChatRouter) ActivatorUtilities.CreateInstance(serviceProvider, chatRouterType);
            var chatContext = serviceProvider.GetRequiredService<ITelegramChatContext>();
            var property = typeof(ChatRouter).GetProperty("Context", BindingFlags.Instance | BindingFlags.Public);
            property!.SetValue(router, chatContext);
            return router;
        };
    }

    private static Func<IServiceProvider, object> ResolveMessageRouterMethod(Type messageRouterType)
    {
        return (serviceProvider) =>
        {
            var router = (MessageRouter) ActivatorUtilities.CreateInstance(serviceProvider, messageRouterType);
            var messageContext = serviceProvider.GetRequiredService<ITelegramMessageContext>();
            var property = typeof(MessageRouter).GetProperty("Context", BindingFlags.Instance | BindingFlags.Public);
            property!.SetValue(router, messageContext);
            return router;
        };
    }
}