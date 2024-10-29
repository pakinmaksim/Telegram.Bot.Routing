using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing;

public class MessageStructure
{
    public static implicit operator MessageStructure(string text)
    {
        return new MessageStructure()
        {
            Text = text
        };
    }
    
    public required string Text { get; init; }
    public InlineKeyboardMarkup? ReplyMarkup { get; set; }
}