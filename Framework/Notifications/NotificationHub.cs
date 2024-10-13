using Microsoft.AspNetCore.SignalR;

namespace Framework.Notifications
{
    //dodati ovde [Authorize] kasnije i testirati da li radi
    public class NotificationHub : Hub
    {
        public static string Url { get; } = "notification-hub";
    }
}
