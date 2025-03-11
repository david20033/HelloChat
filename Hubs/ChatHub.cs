
using System.Security.Claims;
using HelloChat.Data;
using HelloChat.Repositories;
using HelloChat.Repositories.IRepositories;
using Microsoft.AspNetCore.SignalR;
namespace HelloChat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IAppRepository _repository;

        public ChatHub(IAppRepository repository) 
        {
            _repository = repository;
        }
        public async Task SendMessage(string FromId, string ToId, string message)
        {
            await _repository.SendMessage(FromId, ToId, message);
            await Clients.Users(FromId, ToId).SendAsync("ReceiveMessage", FromId, ToId, message);
            //await Clients.All.SendAsync("ReceiveMessage", FromId, ToId, message);
        }
        public async Task GetCurrentUser(string UserId)
        {

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
