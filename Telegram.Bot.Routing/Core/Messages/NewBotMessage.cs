using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Routing.Core.Messages;

public class NewBotMessage
{
    public required string Text { get; set; }
    public ReplyMarkup? ReplyMarkup { get; set; }
    public ParseMode? ParseMode { get; set; }
    public ReplyParameters? ReplyParameters { get; set; }
    public LinkPreviewOptions? LinkPreviewOptions { get; set; }
    public int? MessageThreadId { get; set; }
    public IEnumerable<MessageEntity>? Entities { get; set; }
    public bool DisableNotification { get; set; } = false;
    public bool ProtectContent { get; set; } = false;
    public string? MessageEffectId { get; set; }
    public string? BusinessConnectionId { get; set; }
    public bool AllowPaidBroadcast { get; set; } = false;
    public long? DirectMessagesTopicId { get; set; }
    public SuggestedPostParameters? SuggestedPostParameters { get; set; }
}