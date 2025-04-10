
using System.Reflection.Metadata;
using System.Security.Claims;
using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.Repositories;
using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
namespace HelloChat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IHomeService _homeService;
        private static readonly List<string> _onlineUsers = [];
        private static readonly Dictionary<string,string> _currentUserConversation = [];

        public ChatHub(IHomeService Service) 
        {
            _homeService = Service;
        }
        public async Task SendMessage(string FromId, string ToId, string message)
        {
            var messageId = await _homeService.SendMessageAndReturnItsId(FromId, ToId, message);
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            await Clients.User(FromId).SendAsync("SendMessage",messageId, message);
            if (CurrConversation == null|| CurrConversation!= ToUserConversation)
            {
                if (_onlineUsers.Contains(ToId))
                {
                    await Clients.User(ToId).SendAsync("ReceiveMessageNotification", CurrConversation, message);
                }
                return;
            }
            await Clients.User(ToId).SendAsync("ReceiveMessage",messageId, message);
            await _homeService.SetSeenToLastMessageAndReturnItsId(ToId, Guid.Parse(CurrConversation));
            await Clients.User(FromId).SendAsync("ReceiveSeen",messageId);
        }
        public async Task SendImage(string FromId, string ToId ,string imageName, string base64Image)
        {
            var (ImageId,ImageUrl) = await _homeService.SendImageAndReturnItsIdAndUrl(FromId, ToId, imageName, base64Image);
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            await Clients.User(FromId).SendAsync("SendImage", ImageId, ImageUrl);
            if (CurrConversation == null || CurrConversation != ToUserConversation)
            {
                if (_onlineUsers.Contains(ToId))
                {
                    await Clients.User(ToId).SendAsync("ReceiveMessageNotification", CurrConversation);
                }
                return;
            }
            await Clients.User(ToId).SendAsync("ReceiveImage", ImageId, ImageUrl);
            await _homeService.SetSeenToLastMessageAndReturnItsId(ToId, Guid.Parse(CurrConversation));
            await Clients.User(FromId).SendAsync("ReceiveSeen", ImageId);
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
            await _homeService.DeleteMessageContent(Guid.Parse(MessageId));
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
            await _homeService.SetLocalDelete(Guid.Parse(MessageId));
            await Clients.User(FromId).SendAsync("ReceiveLocalDeleteMessage", MessageId);
        }
        public async Task SendCurrentActiveStatus(string UserId)
        {
            var CurrConversation = GetCurrentUserConversation(UserId);
            if(CurrConversation.IsNullOrEmpty()) return;
            var AnotherUserId = await _homeService.GetAnotherUserIdInConversationAsync(UserId,Guid.Parse(CurrConversation));
            var ActiveString = await _homeService.GetUserActiveString(AnotherUserId);
            await Clients.User(UserId).SendAsync("ReceiveActiveString", ActiveString);
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
            await _homeService.SetMessageReaction(Guid.Parse(MessageId),FromId,ToId,enumReaction);
            var CurrConversation = GetCurrentUserConversation(FromId);
            var ToUserConversation = GetCurrentUserConversation(ToId);
            await Clients.User(FromId).SendAsync("ReceiveMessageReaction", MessageId, enumReaction.ToString(), FromId);
            if (CurrConversation == null || CurrConversation != ToUserConversation)
            {
                return;
            }
            await Clients.User(ToId).SendAsync("ReceiveMessageReaction", MessageId, enumReaction.ToString(), FromId);
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
            var isSeen = await _homeService.isLastMessageSeen(UserId, Guid.Parse(ConversationId));
            if (isSeen) return;
            var messageId = await _homeService.SetSeenToLastMessageAndReturnItsId(UserId, Guid.Parse(ConversationId));
            var OtherUserId = await _homeService
                .GetAnotherUserIdInConversationAsync(UserId, Guid.Parse(ConversationId));
            if(!_currentUserConversation.TryGetValue(OtherUserId, out var currentID)) return;
            if (currentID == ConversationId)
            {
                await Clients.User(OtherUserId).SendAsync("ReceiveSeen", messageId);
            }
        }
        public override async Task OnConnectedAsync()
        {
            string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                 _onlineUsers.Add(userId);
                await _homeService.SetUserActive(userId);
            }
            var FriendIds = await GetFriendIds(userId);
            foreach (var fr in FriendIds)
            {
               await Clients.User(fr).SendAsync("FriendConnected", userId);
            }

            await base.OnConnectedAsync();
        }
        public override  async Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _onlineUsers.Remove(userId);
            await _homeService.SetUserExitActive(userId);
            var onlineFriendIds =  GetOnlineFriends(userId).Result;
            foreach (var fr in onlineFriendIds)
            {
                await Clients.User(fr).SendAsync("FriendDisconnected", userId);
            }

            await base.OnDisconnectedAsync(exception);
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
            return await _homeService.GetUserFriendIds(userId);
        }
        public async Task SendFriendRequestNotification(string ToId)
        {
            await Clients.User(ToId).SendAsync("ReceiveFriendRequestNotification");
        }
        public async Task SendAcceptFriendRequestNotification(string ToId)
        {
            await Clients.User(ToId).SendAsync("ReceiveAcceptFriendRequestNotification");
        }
    }
}
