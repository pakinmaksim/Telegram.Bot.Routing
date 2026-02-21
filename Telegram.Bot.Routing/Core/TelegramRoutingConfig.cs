using System.Reflection;
using System.Text.Json;
using Telegram.Bot.Routing.Registration;
using Telegram.Bot.Routing.Routers;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.InMemory;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Routing.Core;

public class TelegramRoutingConfig
{
    internal bool PoolingEnabled { get; set; }
    internal ParseMode DefaultParseMode { get; set; } = ParseMode.None;
    internal JsonSerializerOptions JsonSerializerOptions { get; set; } = new();
    internal readonly List<Assembly> AssembliesToRegister = [];
    internal string? DefaultChatRouterName;
    internal readonly Dictionary<string, DefinedChatRouter> ChatRouters = [];
    internal string? DefaultBotMessageRouterName;
    internal readonly Dictionary<string, DefinedBotMessageRouter> BotMessageRouters = [];
    

    public TelegramRoutingConfig UsePooling(bool value = true)
    {
        PoolingEnabled = value;
        return this;
    }

    public TelegramRoutingConfig UseParseMode(ParseMode parseMode)
    {
        DefaultParseMode = parseMode;
        return this;
    }

    public TelegramRoutingConfig UseJsonSerializerOptions(JsonSerializerOptions options)
    {
        JsonSerializerOptions = options;
        return this;
    }
    
    public TelegramRoutingConfig RegisterRoutersFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);
        return this;
    }
}