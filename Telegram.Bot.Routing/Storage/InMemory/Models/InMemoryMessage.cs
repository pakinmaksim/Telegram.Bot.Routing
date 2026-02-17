using System.Text.Json;
using Telegram.Bot.Routing.Storage.Models;

namespace Telegram.Bot.Routing.Storage.InMemory.Models;

public class InMemoryMessage : IMessage
{
    public long TelegramChatId { get; set; }
    public int TelegramMessageId { get; set; }
    public long TelegramFromId { get; set; }
    public DateTime Date { get; set; }

    public string? Router { get; set; }
    public JsonDocument? Data { get; set; }
    
    public string? Text { get; set; }
}