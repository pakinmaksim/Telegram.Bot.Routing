using Telegram.Bot.Routing.Core;
using Telegram.Bot.Routing.Extensions;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Routing.Example;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
        // Add ITelegramBotClient interface
        builder.Services.AddSingleton<ITelegramBotClient>(x => 
            new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN")!)
        );
        // Add the routing system
        builder.Services.AddTelegramBotRouting(x =>
        {
            x.UsePooling();
            x.UseParseMode(ParseMode.None);
            x.RegisterRoutersFromAssembly(typeof(Program).Assembly);
        });
        
        var app = builder.Build();
        
        app.MapControllers();
        
        app.Run();
    }
}