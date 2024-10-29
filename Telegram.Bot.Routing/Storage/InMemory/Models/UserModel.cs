using Telegram.Bot.Routing.Storage.Models;

namespace Telegram.Bot.Routing.Storage.InMemory.Models;

public class UserModel : IUser
{
    public long TelegramId { get; set; }
}