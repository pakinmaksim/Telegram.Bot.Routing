using Telegram.Bot.Routing.Storage.Models;

namespace Telegram.Bot.Routing.Storage.InMemory.Models;

public class ChatModel : IChat
{
    public long TelegramId { get; set; }
    public string? RouterName { get; set; }
    public string? RouterData { get; set; }
    public string? RouteName { get; set; }
}