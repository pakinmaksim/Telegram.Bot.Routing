using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Example.Telegram.Greetings;

namespace Telegram.Bot.Routing.Example.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotifyController : Controller
{
    [HttpPost("test")]
    public async Task Index(
        [FromServices] TelegramBotRoutingSystem telegram,
        [FromQuery] long chatId)
    {
        await using var scope = await telegram.CreateChatScope(chatId);
        var context = scope.ServiceProvider.GetRequiredService<ITelegramChatContext>();

        await context.SendMessage("Notification test");
    }
}