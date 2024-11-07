namespace Telegram.Bot.Routing.Binding;

internal enum RouteParameterKind
{
    Unknown,
    Service,
    RouterData,
    UserMessage,
    CallbackQuery,
    CallbackData,
    CancellationToken
}