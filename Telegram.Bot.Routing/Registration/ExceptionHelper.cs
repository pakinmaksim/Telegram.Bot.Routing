namespace Telegram.Bot.Routing.Registration;

public static class ExceptionHelper
{
    public static Exception MultipleChatRouterIndex(Type chatRouterType)
    {
        return new ArgumentException($"Cannot have multiple Index methods in {chatRouterType.Name}");
    }
    public static Exception MultipleDefaultMessageRoutes(Type chatRouterType)
    {
        return new ArgumentException($"Cannot have multiple default message routes in {chatRouterType.Name}");
    }
    public static Exception MessageRouterIndexMissing(Type messageRouterType)
    {
        return new ArgumentException($"Cannot find Index method in {messageRouterType.Name}");
    }
    public static Exception MultipleMessageRouterIndex(Type messageRouterType)
    {
        return new ArgumentException($"Cannot have multiple Index methods in {messageRouterType.Name}");
    }
    public static Exception MultipleDefaultCallbackRoutes(Type messageRouterType)
    {
        return new ArgumentException($"Cannot have multiple default callback routes in {messageRouterType.Name}");
    }
}