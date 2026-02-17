using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Core.Messages;

public class EditBotMessage
{
    public required string Text { get; set; }
    public InlineKeyboardMarkup? ReplyMarkup { get; set; }
    public ParseMode? ParseMode { get; set; }
    public LinkPreviewOptions? LinkPreviewOptions { get; set; }
    public IEnumerable<MessageEntity>? Entities { get; set; }
    public string? BusinessConnectionId { get; set; }
}