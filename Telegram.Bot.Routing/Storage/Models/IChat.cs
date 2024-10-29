namespace Telegram.Bot.Routing.Storage.Models;

public interface IChat
{
    public long TelegramId { get; set; }
    public string? RouterName { get; set; }
    public string? RouterData { get; set; }
    public string? RouteName { get; set; }

    public bool IsPrivate => TelegramId > 0;
}