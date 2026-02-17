using System.Text.Json;

namespace Telegram.Bot.Routing.Storage.Models;

public class UnsendMessage : IMessage
{
    public const int UnsentMessageId = -1;
    
    public long TelegramChatId { get; set; }
    public int TelegramMessageId { get; set; } = UnsentMessageId;
    public long TelegramFromId { get; set; }
    public DateTime Date { get; set; }
    public string? Router { get; set; }
    public JsonDocument? Data { get; set; }
}