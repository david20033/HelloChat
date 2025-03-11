
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
        private static readonly List<string> _onlineUsers = [];

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
                _onlineUsers.Add(userId);
            }
            var FriendIds = GetFriendIds(userId).Result;
            foreach (var fr in FriendIds)
            {
                Clients.User(fr).SendAsync("FriendConnected", userId);
            }

            return base.OnConnectedAsync();
        }
        public override  Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _onlineUsers.Remove(userId);
            var onlineFriendIds =  GetOnlineFriends(userId).Result;
            foreach (var fr in onlineFriendIds)
            {
                 Clients.User(fr).SendAsync("FriendDisconnected", userId);
            }

            return base.OnDisconnectedAsync(exception);
        }
        public async Task GetOnlineUsers(List<string> friendIds)
        {
            var OnlineFriendIds = _onlineUsers.Intersect(friendIds).ToList();
            await Clients.Caller.SendAsync("OnlineUsers", OnlineFriendIds);
        }

        public async Task<List<string>> GetOnlineFriends(string userId)
        {
            var friendIds = await GetFriendIds(userId);
            var onlineFriendIds = _onlineUsers.Intersect(friendIds).ToList();

            return onlineFriendIds;
        }

        private async Task<List<string>> GetFriendIds(string userId)
        {
            return await _repository.GetUserFriendIds(userId);
        }
    }
}
