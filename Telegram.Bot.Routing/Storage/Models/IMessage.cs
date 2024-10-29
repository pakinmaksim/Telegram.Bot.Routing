namespace Telegram.Bot.Routing.Storage.Models;

public interface IMessage
{
    public const int CreationId = int.MinValue;
    
    public long ChatTelegramId { get; set; }
    public int TelegramId { get; set; }
    public string Text { get; set; }
    public string? RouterName { get; set; }
    public string? RouterData { get; set; }
    
    public bool IsCreated => TelegramId != CreationId;
}