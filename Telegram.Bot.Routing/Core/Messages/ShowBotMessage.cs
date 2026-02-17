using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Core.Messages;

public class ShowBotMessage
{
    public required string Text { get; set; }
    public InlineKeyboardMarkup? ReplyMarkup { get; set; }
    public ParseMode? ParseMode { get; set; }
    public LinkPreviewOptions? LinkPreviewOptions { get; set; }
    public IEnumerable<MessageEntity>? Entities { get; set; }
    public string? BusinessConnectionId { get; set; }

    public NewBotMessage ToNewBotMessage()
    {
        return new NewBotMessage()
        {
            Text = Text,
            ParseMode = ParseMode,
            ReplyMarkup = ReplyMarkup,
            LinkPreviewOptions = LinkPreviewOptions,
            Entities = Entities,
            BusinessConnectionId = BusinessConnectionId
        };
    }

    public EditBotMessage ToEditBotMessage()
    {
        return new EditBotMessage()
        {
            Text = Text,
            ParseMode = ParseMode,
            ReplyMarkup = ReplyMarkup,
            LinkPreviewOptions = LinkPreviewOptions,
            Entities = Entities,
            BusinessConnectionId = BusinessConnectionId
        };
    }
}