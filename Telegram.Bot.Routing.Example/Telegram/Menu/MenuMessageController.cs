namespace Telegram.Bot.Routing.Example.Telegram.Menu;

// public class MenuMessageController
// {
//     private readonly TelegramContext _telegram;
//
//     public MenuMessageRouter(TelegramContext telegram)
//     {
//         _telegram = telegram;
//     }
//
//     [CallbackRoute]
//     public NavigationMessageData Index()
//     {
//         return new NavigationMessageData()
//         {
//             Text = "Добро пожаловать в меню!",
//             Keyboard =
//             [
//                 [Action("Мой сервер", "my_server")],
//                 [Action("Подменю", "submenu")],
//                 [Action("Тарифы", "tariffs")],
//                 [Link("О нас", "https://example.com/")],
//             ]
//         };
//     }
//
//     [CallbackRoute("my_server")]
//     public async Task<NavigationMessageData> MyServer()
//     {
//         
//     }
//
//     [CallbackRoute("tariffs")]
//     public async Task<NavigationMessageData> Tariffs(PaginationParams pagination)
//     {
//         
//     }
// }