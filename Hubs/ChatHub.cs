
using System.Reflection.Metadata;
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
        private static readonly Dictionary<string,string> _currentUserConversation = [];

        public ChatHub(IAppRepository repository) 
        {
            _repository = repository;
        }
        public async Task SendMessage(string FromId, string ToId, string message)
        {
            await _repository.SendMessage(FromId, ToId, message);
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            if (CurrConversation == null|| CurrConversation!= ToUserConversation)
            {
                await Clients.Users(FromId, ToId).SendAsync("OnlySendMessage", FromId, ToId, message);
                return;
            }
            await Clients.Users(FromId, ToId).SendAsync("ReceiveMessage", FromId, ToId, message);
        }
        public async Task SendTyping(string FromId, string ToId)
        {
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            if (CurrConversation == null || CurrConversation != ToUserConversation)
            {
                return;
            }
            await Clients.User(ToId).SendAsync("ReceiveTyping");
        }
        private  string? GetCurrentUserConversation(string UserId)
        {
            _currentUserConversation.TryGetValue(UserId, out var conversation);
            return conversation;
        }
        public void SetCurrentUserConversation(string UserId, string ConversationId)
        {
            if(_currentUserConversation.ContainsKey(UserId))
            {
                _currentUserConversation[UserId] = ConversationId;
            }
            else
            {
                _currentUserConversation.Add(UserId, ConversationId);
            }
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
