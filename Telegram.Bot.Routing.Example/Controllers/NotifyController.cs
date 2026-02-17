using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Routing.Contexts.Chats;
using Telegram.Bot.Routing.Core;

namespace Telegram.Bot.Routing.Example.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotifyController : Controller
{
    [HttpPost("test")]
    public async Task Index(
        [FromServices] TelegramRoutingSystem telegram,
        [FromQuery] long chatId)
    {
        await using var scope = telegram.SetupTelegramScope();
        var context = await scope.GetChatContext(chatId);

        await context.SendMessage("Notification test");
    }
}