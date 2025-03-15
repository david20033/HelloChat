
using System.Reflection.Metadata;
using System.Security.Claims;
using HelloChat.Data;
using HelloChat.Enums;
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
            var messageId = await _repository.SendMessageAndReturnItsId(FromId, ToId, message);
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            await Clients.User(FromId).SendAsync("SendMessage",messageId, message);
            if (CurrConversation == null|| CurrConversation!= ToUserConversation)
            {
                return;
            }
            await Clients.User(ToId).SendAsync("ReceiveMessage",messageId, message);
            await _repository.SetSeenToLastMessageAndReturnItsId(ToId, Guid.Parse(CurrConversation));
            await Clients.User(FromId).SendAsync("ReceiveSeen",messageId);
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
        public async Task SendStopTyping(string FromId,string ToId)
        {
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            if (CurrConversation == null || CurrConversation != ToUserConversation)
            {
                return;
            }
            await Clients.User(ToId).SendAsync("ReceiveStopTyping");
        }
        public async Task SendGlobalDeleteMessage(string FromId, string ToId,string MessageId)
        {
            await _repository.DeleteMessageContent(Guid.Parse(MessageId));
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            await Clients.User(FromId).SendAsync("ReceiveGlobalDeleteMessage",MessageId);
            if (CurrConversation == null || CurrConversation != ToUserConversation)
            {
                return;
            }
            await Clients.User(ToId).SendAsync("ReceiveGlobalDeleteMessage", MessageId);
        }
        public async Task SendLocalDeleteMessage(string FromId, string MessageId)
        {
            await _repository.SetLocalDeleted(Guid.Parse(MessageId));
            await Clients.User(FromId).SendAsync("ReceiveLocalDeleteMessage", MessageId);
        }
        private  string? GetCurrentUserConversation(string UserId)
        {
            _currentUserConversation.TryGetValue(UserId, out var conversation);
            return conversation;
        }
        public async Task SendMessageReaction(string MessageId, string Reaction, string FromId, string ToId)
        {
            var enumReaction = MessageReaction.None;
            switch (Reaction)
            {
                case "Love":
                    enumReaction = MessageReaction.Love;
                    break;
                case "Like":
                    enumReaction = MessageReaction.Like;
                    break;
                case "Laugh":
                    enumReaction = MessageReaction.Laugh;
                    break;
                case "Smile":
                    enumReaction = MessageReaction.Smile;
                    break;
                case "Angry":
                    enumReaction = MessageReaction.Angry;
                    break;
                default: return;
            }
            await _repository.SetMessageReaction(Guid.Parse(MessageId),FromId,ToId,enumReaction);
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            await Clients.User(FromId).SendAsync("ReceiveMessageReaction", MessageId, enumReaction.ToString());
            if (CurrConversation == null || CurrConversation != ToUserConversation)
            {
                return;
            }
            await Clients.User(ToId).SendAsync("ReceiveMessageReaction", MessageId, enumReaction.ToString());
        }
        public async Task SetCurrentUserConversation(string UserId, string ConversationId)
        {
            if(_currentUserConversation.ContainsKey(UserId))
            {
                _currentUserConversation[UserId] = ConversationId;
            }
            else
            {
                _currentUserConversation.Add(UserId, ConversationId);
            }
            var isSeen = await _repository.isLastMessageSeen(UserId, Guid.Parse(ConversationId));
            if (isSeen) return;
            var messageId = await _repository.SetSeenToLastMessageAndReturnItsId(UserId, Guid.Parse(ConversationId));
            var OtherUserId = await _repository
                .GetAnotherUserIdInConversationAsync(UserId, Guid.Parse(ConversationId));
            if(!_currentUserConversation.TryGetValue(OtherUserId, out var currentID)) return;
            if (currentID == ConversationId)
            {
                await Clients.User(OtherUserId).SendAsync("ReceiveSeen", messageId);
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
