using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Contexts;

public class TelegramScopeManager
{
    public Type ContextType { get; private set; } = typeof(TelegramContext);
    public Update? Update = null;
    
    public void SetCurrentContext<T>(Update? update)
        where T : TelegramContext
    {
        ContextType = typeof(T);
        Update = update;
    }
}