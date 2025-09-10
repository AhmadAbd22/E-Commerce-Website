using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ECommerceWebsite.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }

        // Send a notification to a specific user
        public async Task SendNotificationToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        // Send an order status update to a user
        public async Task SendOrderStatusUpdate(string userId, string orderId, string status)
        {
            await Clients.User(userId).SendAsync("ReceiveOrderStatusUpdate", orderId, status);
        }

        // Notify all admins
        public async Task NotifyAdmins(string message)
        {
            await Clients.Group("Admins").SendAsync("AdminNotification", message);
        }

        // Broadcast
        public async Task BroadcastNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
