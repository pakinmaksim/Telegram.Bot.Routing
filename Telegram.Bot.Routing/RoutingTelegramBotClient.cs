using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing;

internal class RoutingTelegramBotClient : TelegramBotClient
{
    private const int MaxMessagesPerMinuteSamePrivateChat = 60;
    private const int MaxMessagesPerMinuteSameGroup = 20;
    private const int MaxMessagesPerMinute = 1800;

    private readonly TimeSpan _samePrivateChatTimeGap = TimeSpan.FromMinutes(1.0 / MaxMessagesPerMinuteSamePrivateChat);
    private readonly TimeSpan _sameGroupTimeGap = TimeSpan.FromMinutes(1.0 / MaxMessagesPerMinuteSameGroup);
    private readonly TimeSpan _defaultTimeGap = TimeSpan.FromMinutes(1.0 / MaxMessagesPerMinute);

    private readonly object _lockObject = new();
    private readonly Dictionary<long, DateTimeOffset> _chatNextRequestTimes = new();
    private DateTimeOffset _defaultNextRequestTime = DateTimeOffset.MinValue;
    
    public RoutingTelegramBotClient(TelegramBotClientOptions options, HttpClient? httpClient = null) 
        : base(options, httpClient) { }

    public RoutingTelegramBotClient(string token, HttpClient? httpClient = null) 
        : base(token, httpClient) { }

    public override async Task<TResponse> MakeRequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
    {
        var isChatPostRequest = request is 
            SendMessageRequest or 
            EditMessageTextRequest or 
            EditMessageReplyMarkupRequest or 
            DeleteMessageRequest;
        if (isChatPostRequest && request is IChatTargetable chatTargetable)
        {
            await WaitRequestGap(chatTargetable.ChatId, cancellationToken);
        }

        try
        {
            return await base.MakeRequestAsync(request, cancellationToken);
        }
        catch (RequestException e)
        {
            await Task.Delay(500, cancellationToken); // Adding a delay to handle rate limits gracefully
            return await base.MakeRequestAsync(request, cancellationToken);
        }
    }
    
    private async Task WaitRequestGap(ChatId chatId, CancellationToken ct = default)
    {
        TimeSpan delay;
        lock (_lockObject)
        {
            var now = DateTimeOffset.Now;

            // Default Rate Limiting
            if (_defaultNextRequestTime > now)
            {
                delay = _defaultNextRequestTime - now;
                _defaultNextRequestTime += _defaultTimeGap;
            }
            else
            {
                delay = TimeSpan.Zero;
                _defaultNextRequestTime = now + _defaultTimeGap;
            }

            // Chat-specific Rate Limiting
            var chatIdValue = chatId.Identifier ?? default;
            var timeGap = chatIdValue < 0 ? _sameGroupTimeGap : _samePrivateChatTimeGap;

            _chatNextRequestTimes.TryAdd(chatIdValue, DateTimeOffset.MinValue);
            
            var nextRequestTime = _chatNextRequestTimes[chatIdValue];
            if (nextRequestTime > now)
            {
                var additionalDelay = nextRequestTime - now;
                nextRequestTime += timeGap;
                
                delay = delay > additionalDelay ? delay : additionalDelay;
            }
            else
            {
                nextRequestTime = now + timeGap;
            }
            _chatNextRequestTimes[chatIdValue] = nextRequestTime;
        }

        if (delay > TimeSpan.Zero) await Task.Delay(delay, ct);
    }
}