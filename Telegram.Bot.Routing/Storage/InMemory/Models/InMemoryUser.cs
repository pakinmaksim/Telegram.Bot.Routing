using Telegram.Bot.Routing.Storage.Models;

namespace Telegram.Bot.Routing.Storage.InMemory.Models;

public class InMemoryUser : IUser
{
    public long TelegramId { get; set; }

    public string? Username { get; set; }
}