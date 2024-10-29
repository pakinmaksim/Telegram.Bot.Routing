using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Routing.Registration;

namespace Telegram.Bot.Routing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBotRouting(
        this IServiceCollection services, 
        Action<TelegramBotRoutingOptions> optionsAction)
    {
        var options = new TelegramBotRoutingOptions();
        optionsAction.Invoke(options);
        ServiceRegistrar.AddRouteClasses(services, options);
        ServiceRegistrar.AddRequiredServices(services, options);
        return services;
    }
}