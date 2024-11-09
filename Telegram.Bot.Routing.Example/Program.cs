using Telegram.Bot.Routing.Example.Hosting;
using Telegram.Bot.Routing.Extensions;

namespace Telegram.Bot.Routing.Example;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
        // Add ITelegramBotClient interface
        builder.Services.AddSingleton<ITelegramBotClient>(x => new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN")!));
        // Add routing system
        builder.Services.AddTelegramBotRouting(x =>
        {
            x.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        // Add long pooling
        builder.Services.AddHostedService<TelegramHostingService>();
        
        var app = builder.Build();
        
        app.MapControllers();
        
        app.Run();
    }
}