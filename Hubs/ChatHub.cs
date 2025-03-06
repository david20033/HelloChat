
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
namespace HelloChat.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string ReceiverUserId, string message)
        {
            await Clients.User(ReceiverUserId).SendAsync("ReceiveMessage", message);
        }
        public override Task OnConnectedAsync()
        {
            string userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            return base.OnConnectedAsync();
        }
    }
}
