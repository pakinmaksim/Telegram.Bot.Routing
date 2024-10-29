using System.Collections.Concurrent;
using Telegram.Bot.Routing.Storage.InMemory.Models;
using Telegram.Bot.Routing.Storage.Models;
using Telegram.Bot.Types;

namespace Telegram.Bot.Routing.Storage.InMemory;

public class InMemoryTelegramStorage : ITelegramStorage
{
    private static readonly ConcurrentDictionary<long, UserModel> Users = new();
    private static readonly ConcurrentDictionary<long, ChatModel> Chats = new();
    private static readonly ConcurrentDictionary<(long, int), MessageModel> Messages = new();

    public Task<IUser> ConstructUser(User original, CancellationToken ct = default)
    {
        return Task.FromResult<IUser>(new UserModel()
        {
            TelegramId = original.Id,
        });
    }

    public Task<IUser?> GetUser(long userId, CancellationToken ct = default)
    {
        var model = Users.GetValueOrDefault(userId);
        return Task.FromResult<IUser?>(model is null ? null : Copy(model));
    }

    public Task SetUser(IUser user, CancellationToken ct = default)
    {
        if (Users.TryGetValue(user.TelegramId, out var exists))
        {
            exists.TelegramId = user.TelegramId;
        }
        else
        {
            Users.TryAdd(user.TelegramId, (UserModel) user);
        }
        return Task.CompletedTask;
    }
    
    public Task<IChat> ConstructChat(Chat original, CancellationToken ct = default)
    {
        return Task.FromResult<IChat>(new ChatModel()
        {
            TelegramId = original.Id,
            RouterName = null,
            RouterData = null,
            RouteName = null
        });
    }
    public Task<IChat?> GetChat(long chatId, CancellationToken ct = default)
    {
        var model = Chats.GetValueOrDefault(chatId);
        return Task.FromResult<IChat?>(model is null ? null : Copy(model));
    }
    public Task SetChat(IChat chat, CancellationToken ct = default)
    {
        if (Chats.TryGetValue(chat.TelegramId, out var exists))
        {
            exists.TelegramId = chat.TelegramId;
            exists.RouterName = chat.RouterName;
            exists.RouterData = chat.RouterData;
            exists.RouteName = chat.RouteName;
        }
        else
        {
            Chats.TryAdd(chat.TelegramId, (ChatModel) chat);
        }
        return Task.CompletedTask;
    }
    
    public Task<IMessage> ConstructMessage(Message original, CancellationToken ct = default)
    {
        return Task.FromResult<IMessage>(new MessageModel()
        {
            ChatTelegramId = original.Chat.Id,
            TelegramId = original.MessageId,
            Text = original.Text ?? "",
            RouterName = null,
            RouterData = null
        });
    }
    public Task<IMessage> ConstructMessage(long chatId, int messageId, string routerName, string? routerData, CancellationToken ct = default)
    {
        return Task.FromResult<IMessage>(new MessageModel()
        {
            ChatTelegramId = chatId,
            TelegramId = messageId,
            Text = "",
            RouterName = routerName,
            RouterData = routerData
        });
    }
    public Task<IMessage?> GetMessage(long chatId, int messageId, CancellationToken ct = default)
    {
        var key = (chatTelegramId: chatId, telegramId: messageId);
        
        var model = Messages.GetValueOrDefault(key);
        return Task.FromResult<IMessage?>(model is null ? null : Copy(model));
    }

    public Task SetMessage(IMessage message, CancellationToken ct = default)
    {
        var key = (message.ChatTelegramId, message.TelegramId);
        if (Messages.TryGetValue(key, out var exists))
        {
            exists.ChatTelegramId = message.ChatTelegramId;
            exists.TelegramId = message.TelegramId;
            exists.Text = message.Text;
            exists.RouterName = message.RouterName;
            exists.RouterData = message.RouterData;
        }
        else
        {
            Messages.TryAdd(key, (MessageModel) message);
        }
        return Task.CompletedTask;
    }


    private UserModel Copy(UserModel user)
    {
        return new UserModel()
        {
            TelegramId = user.TelegramId
        };
    }

    private ChatModel Copy(ChatModel chat)
    {
        return new ChatModel()
        {
            TelegramId = chat.TelegramId,
            RouterName = chat.RouterName,
            RouterData = chat.RouterData,
            RouteName = chat.RouteName
        };
    }

    private MessageModel Copy(MessageModel chat)
    {
        return new MessageModel()
        {
            ChatTelegramId = chat.ChatTelegramId,
            TelegramId = chat.TelegramId,
            Text = chat.Text,
            RouterName = chat.RouterName,
            RouterData = chat.RouterData,
        };
    }
}