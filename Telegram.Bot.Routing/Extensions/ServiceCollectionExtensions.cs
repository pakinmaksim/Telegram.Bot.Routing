using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Core;
using Telegram.Bot.Routing.Registration;

namespace Telegram.Bot.Routing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBotRouting(
        this IServiceCollection services, 
        Action<TelegramRoutingConfig> configAction)
    {
        var config = new TelegramRoutingConfig();
        configAction.Invoke(config);
        ServiceRegistrar.AddRouteClasses(services, config);
        ServiceRegistrar.AddRequiredServices(services, config);
        return services;
    }
}