using System.Reflection;
using Telegram.Bot.Routing.Binding;
using Telegram.Bot.Routing.Storage;
using Telegram.Bot.Routing.Storage.InMemory;
using Telegram.Bot.Routing.Storage.Serializers;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Routing.Registration;


public class TelegramBotRoutingOptions
{
    internal Type ClientType = typeof(RoutingTelegramBotClient);
    internal Type StorageType = typeof(InMemoryTelegramStorage);
    internal Type RouteDataSerializerType = typeof(NewtonsoftJsonRouteDataSerializer);
    internal readonly List<Assembly> AssembliesToRegister = [];
    internal string? DefaultChatRouterName = null;
    internal readonly Dictionary<string, DefinedChatRouter> ChatRouters = new();
    internal string? DefaultMessageRouterName = null;
    internal readonly Dictionary<string, DefinedMessageRouter> MessageRouters = new();
    public ParseMode DefaultParseMode { get; set; } = ParseMode.MarkdownV2;

    public TelegramBotRoutingOptions UseClient<T>() where T : ITelegramBotClient
    {
        ClientType = typeof(T);
        return this;
    }
    public TelegramBotRoutingOptions UseStorage<T>() where T : ITelegramStorage
    {
        StorageType = typeof(T);
        return this;
    }
    public TelegramBotRoutingOptions UseRouteDataSerializer<T>() where T : IRouteDataSerializer
    {
        RouteDataSerializerType = typeof(T);
        return this;
    }
    public TelegramBotRoutingOptions RegisterServicesFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);
        return this;
    }
}