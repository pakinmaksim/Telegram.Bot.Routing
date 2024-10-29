using Telegram.Bot.Routing.Storage.Models;

namespace Telegram.Bot.Routing.Storage.InMemory.Models;

public class MessageModel : IMessage
{
    public long ChatTelegramId { get; set; }
    public int TelegramId { get; set; }
    public string Text { get; set; } = null!;
    public string? RouterName { get; set; }
    public string? RouterData { get; set; }
    
    public void CopyFrom(IMessage message)
    {
        ChatTelegramId = message.ChatTelegramId;
        TelegramId = message.TelegramId;
        Text = message.Text;
        RouterName = message.RouterName;
        RouterData = message.RouterData;
    }
}