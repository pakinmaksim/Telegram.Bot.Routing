using System.Text.Json;
using Telegram.Bot.Routing.Storage.Models;

namespace Telegram.Bot.Routing.Storage.InMemory.Models;

public class InMemoryChat : IChat
{
    public long TelegramId { get; set; }
    
    public string? Router { get; set; }
    public JsonDocument? Data { get; set; }

    public string? Title { get; set; }
}