using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Contexts;
using Telegram.Bot.Routing.Contexts.BotMessages;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Core;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.InMemory;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Registration;

public class ServiceRegistrar
{
    public static IServiceCollection AddRouteClasses(
        IServiceCollection services,
        TelegramRoutingConfig config)
    {
        config.DefaultChatRouterName = null;
        config.ChatRouters.Clear();
        config.DefaultBotMessageRouterName = null;
        config.BotMessageRouters.Clear();

        var chatRouters = new List<DefinedChatRouter>();
        var botMessageRouters = new List<DefinedBotMessageRouter>();
        
        var possibleTypes = config.AssembliesToRegister
            .SelectMany(x => x.DefinedTypes.Where(y => y is {IsClass: true, IsAbstract: false}));
        foreach (var type in possibleTypes)
        {
            if (TryCreateChatRouter(type) is { } chatRouter)
                chatRouters.Add(chatRouter);
            else if (TryCreateMessageRouter(type) is {} botMessageRouter) 
                botMessageRouters.Add(botMessageRouter);
        }

        // Chat Routers
        var duplicatedChats = chatRouters
            .SelectMany(x => x.Names)
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();
        if (duplicatedChats.Length > 0)
        {
            throw new InvalidOperationException(
                $"Duplicated chat routers names found: {string.Join(", ", duplicatedChats)}");
        }
        var defaultChatRouters = chatRouters.Where(x => x.IsDefault).ToArray();
        config.DefaultChatRouterName = defaultChatRouters.Length switch
        {
            0 => null,
            1 => defaultChatRouters[0].Name,
            _ => throw new InvalidOperationException(
                $"Several default chat routers found: {string.Join(", ", defaultChatRouters.Select(x => x.Name))}")
        };
        foreach (var chatRouter in chatRouters)
        {
            config.ChatRouters.Add(chatRouter.Name, chatRouter);
            services.AddScoped(chatRouter.Type);
            services.AddKeyedScoped(typeof(ChatRouter), chatRouter.Name, (x, _) => x.GetRequiredService(chatRouter.Type));
        }

        // Bot Message Routers
        var duplicatedBotMessage = botMessageRouters
            .SelectMany(x => x.Names)
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();
        if (duplicatedBotMessage.Length > 0)
        {
            throw new InvalidOperationException(
                $"Duplicated bot message routers names found: {string.Join(", ", duplicatedBotMessage)}");
        }
        var defaultBotMessageRouters = botMessageRouters.Where(x => x.IsDefault).ToArray();
        config.DefaultBotMessageRouterName = defaultBotMessageRouters.Length switch
        {
            0 => null,
            1 => defaultBotMessageRouters[0].Name,
            _ => throw new InvalidOperationException(
                $"Several default chat routers found: {string.Join(", ", defaultBotMessageRouters.Select(x => x.Name))}")
        };
        foreach (var botMessageRouter in botMessageRouters)
        {
            config.BotMessageRouters.Add(botMessageRouter.Name, botMessageRouter);
            services.AddScoped(botMessageRouter.Type);
            services.AddKeyedScoped(typeof(BotMessageRouter), botMessageRouter.Name, (x, _) => x.GetRequiredService(botMessageRouter.Type));
        }
        
        return services;
    }

    public static IServiceCollection AddRequiredServices(
        IServiceCollection services,
        TelegramRoutingConfig config)
    {
        services.AddSingleton(config);
        if (config.StorageType == typeof(InMemoryTelegramStorage))
            services.AddSingleton<InMemoryTelegramStorage>();
        services.AddScoped(typeof(ITelegramStorage), x => x.GetRequiredService(config.StorageType));
        
        services.AddSingleton<TelegramRoutingSystem>();
        if (config.PoolingEnabled)
        {
            services.AddSingleton<TelegramHostingService>();
            services.AddHostedService(x => x.GetRequiredService<TelegramHostingService>());
        }

        return services;
    }

    private static DefinedChatRouter? TryCreateChatRouter(Type type)
    {
        if (!typeof(ChatRouter).IsAssignableFrom(type))
            return null;
        
        var attribute = type.GetCustomAttribute<ChatRouterAttribute>();
        attribute ??= new ChatRouterAttribute(type.Name, isDefault: false, legacyNames: []);

        var currentType = type;
        Type? dataType = null;
        while (true)
        {
            currentType =  currentType.BaseType;

            // If that end of inheritance - wtf
            if (currentType == null)
                break;
            // If the end of inheritance is ChatRouter - all good, break
            if (currentType == typeof(ChatRouter))
                break;
            
            // If that's not the end - check DataType of router
            if (!currentType.IsGenericType)
                continue;
            var genericDefinition = currentType.GetGenericTypeDefinition();
            if (genericDefinition != typeof(ChatRouter<>))
                continue;
            
            dataType = currentType.GetGenericArguments()[0];
        }
        
        return new DefinedChatRouter
        {
            Name = attribute.Name,
            LegacyNames = attribute.LegacyNames
                .Where(x => x != attribute.Name)
                .Distinct()
                .ToArray(),
            Type = type,
            DataType = dataType,
            IsDefault = attribute.IsDefault
        };
    }

    private static DefinedBotMessageRouter? TryCreateMessageRouter(Type type)
    {
        if (!typeof(BotMessageRouter).IsAssignableFrom(type))
            return null;
        
        var attribute = type.GetCustomAttribute<BotMessageRouterAttribute>();
        attribute ??= new BotMessageRouterAttribute(type.Name, isDefault: false, legacyNames: []);
        
        var currentType = type;
        Type? dataType = null;
        while (true)
        {
            currentType =  currentType.BaseType;

            // If that end of inheritance - wtf
            if (currentType == null)
                break;
            // If the end of inheritance is MessageRouter - all good, break
            if (currentType == typeof(BotMessageRouter))
                break;
            
            // If that's not the end - check DataType of router
            if (!currentType.IsGenericType)
                continue;
            var genericDefinition = currentType.GetGenericTypeDefinition();
            if (genericDefinition != typeof(BotMessageRouter<>))
                continue;
            
            dataType = currentType.GetGenericArguments()[0];
        }
        
        return new DefinedBotMessageRouter
        {
            Name = attribute.Name,
            LegacyNames = attribute.LegacyNames
                .Where(x => x != attribute.Name)
                .Distinct()
                .ToArray(),
            Type = type,
            DataType = dataType,
            IsDefault = attribute.IsDefault
        };
    }
}