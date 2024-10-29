using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Contexts;

public class TelegramContextController
{
    private readonly IServiceProvider _serviceProvider;
    private Type _currentContextType = typeof(TelegramContext);
    private Update? _update = null;
    
    public TelegramContextController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void SetCurrentContext<T>(Update? update)
        where T : TelegramContext
    {
        _currentContextType = typeof(T);
        _update = update;
    }
    
    public TelegramContext GetCurrentContext()
    {
        var context = (TelegramContext) ActivatorUtilities.CreateInstance(_serviceProvider, _currentContextType);
        context.Update = _update;
        return context;
    }
}