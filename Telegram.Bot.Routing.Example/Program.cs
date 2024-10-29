using Telegram.Bot.Routing.Example.Hosting;
using Telegram.Bot.Routing.Extensions;

namespace Telegram.Bot.Routing.Example;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add ITelegramBotClient interface
        builder.Services.AddSingleton<ITelegramBotClient>(x => new TelegramBotClient("TOKEN"));
        
        // Add routing system
        builder.Services.AddTelegramBotRouting(x =>
        {
            x.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        
        // Add long pooling
        builder.Services.AddHostedService<TelegramHostingService>();
        
        var app = builder.Build();
        app.Run();
    }
}