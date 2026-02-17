using System.Text.Json;

namespace Telegram.Bot.Routing.Storage.Models;

public interface IChat
{
    public long TelegramId { get; set; }
    
    public string? Router { get; set; }
    public JsonDocument? Data { get; set; }
}