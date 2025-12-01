using HelloChat.Data;
using HelloChat.Models;
using HelloChat.Repositories;
using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using NuGet.Protocol.Core.Types;

namespace HelloChat.Services
{
    public class FriendService : IFriendService
    {
        //userrepo messrepo, conversation repo
        private readonly IUserRepository _userRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IConversationRepository _conversationRepo;
        private readonly IFriendRepository _friendRepo;
        private readonly INotificationRepository _notificationRepo;
        public FriendService(
            IUserRepository userRepo, 
            IMessageRepository messageRepo, 
            IConversationRepository conversationRepo,
            IFriendRepository friendRepo,
            INotificationRepository notificationRepository)
        {
            _userRepo = userRepo;
            _messageRepo = messageRepo;
            _conversationRepo = conversationRepo;
            _friendRepo = friendRepo;
            _notificationRepo = notificationRepository;
        }

        public async Task<List<FriendsViewModel>> GetFriendsViewModelAsync(string CurrentUserId)
        {
            var Friendships = await _friendRepo.GetUserFriendshipsAsync(CurrentUserId);
            List<FriendsViewModel> FriendsList = [];
            foreach (var f in Friendships)
            {
                ApplicationUser user = null;
                if (f.User1Id == CurrentUserId)
                {
                    user = await _userRepo.GetUserByIdAsync(f.User2Id);
                }
                else
                {
                    user = await _userRepo.GetUserByIdAsync(f.User1Id);
                }
                var Messages = await _messageRepo.GetAllReceivedMessagesAsync(CurrentUserId, user.Id);
                var LastMessage = await _messageRepo.GetLastMessageAsync(CurrentUserId, user.Id);
                string LastMessageString = null;
                if (LastMessage?.From_id == CurrentUserId)
                {
                    LastMessageString = "You: " + LastMessage.Content;
                }
                else if (LastMessage != null)
                {
                    LastMessageString = LastMessage.Content;
                }
                var Message = Messages
                    .OrderByDescending(m => m.CreatedDate)
                    .FirstOrDefault();
                var FriendModel = new FriendsViewModel
                {
                    lastMessage = LastMessageString,
                    Name = $"{user.FirstName} {user.LastName}",
                    ProfileImageUrl = user.ProfilePicturePath ?? "",
                    sentTime = LastMessage?.CreatedDate,
                    UserId = user.Id,
                    ConversationId = Message?.ConversationId,
                    isLastMessageSeen = Message?.isSeen
                };
                if (await _conversationRepo.GetConversationAsync(CurrentUserId, user.Id) == null)
                {
                    var el = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        User1Id = CurrentUserId,
                        User2Id = user.Id,
                    };
                    FriendModel.ConversationId = el.Id;
                    await _conversationRepo.AddConversationAsync(el);
                }
                else if (FriendModel.ConversationId == null)
                {
                    var el = await _conversationRepo.GetConversationAsync(CurrentUserId, user.Id);
                    FriendModel.ConversationId = el?.Id;
                }
                FriendsList.Add(FriendModel);
            }
            return FriendsList;
        }

        public async Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query)
        {
            return await _userRepo.GetUsersBySearchQuery(query);
        }

        public async Task<List<string>> GetUserFriendIds(string UserId)
        {

            var Friendships = await _friendRepo.GetUserFriendshipsAsync(UserId);
            List<string> FriendsList = [];
            foreach (var f in Friendships)
            {
                ApplicationUser us = null;
                if (f.User1Id == UserId)
                {
                    us = await _userRepo.GetUserByIdAsync(f.User2Id);
                }
                else
                {
                    us = await _userRepo.GetUserByIdAsync(f.User1Id);
                }
                FriendsList.Add(us.Id);
            }
            return FriendsList;
        }

        public async Task<List<RecommendedFriendViewModel>> MapFromRecommendationResultToRecommendedFriendViewModel(List<RecommendationResult> recommendations)
        {
            List<RecommendedFriendViewModel> Result = new List<RecommendedFriendViewModel>();
            foreach (var rec in recommendations)
            {
                var user = await _userRepo.GetUserByIdAsync(rec.UserId.ToString());
                if (user == null) continue;
                Result.Add(new RecommendedFriendViewModel
                {
                    UserId = user.Id,
                    Name = $"{user.FirstName} {user.LastName}",
                    ProfileImageUrl = user.ProfilePicturePath ?? "",
                    Reason = $"{rec.Reasoning}"
                });
            }
            return Result;
        }
        public async Task AcceptFriendRequest(string FromId, string ToId)
        {
            var FromUser = await _userRepo.GetUserByIdAsync(FromId);
            var ToUser = await _userRepo.GetUserByIdAsync(ToId);
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Content = FromUser.FirstName + " " + FromUser.LastName + " Accepted your friend request",
                HrefId = FromId,
                ApplicationUser = ToUser,
                ApplicationUserId = ToId
            };
            await _notificationRepo.AddNotificationAsync(notification);
            await _friendRepo.AcceptFriendRequest(FromId, ToId);
        }

        public async Task DeleteFriend(string FromId, string ToId)
        {
            await _friendRepo.DeleteFriend(FromId, ToId);
        }

        public async Task DeleteFriendRequest(string FromId, string ToId)
        {
            await _friendRepo.DeleteFriendRequest(FromId, ToId);
        }

        public async Task SendFriendRequest(string FromId, string ToId)
        {
            var FromUser = await _userRepo.GetUserByIdAsync(FromId);
            var ToUser = await _userRepo.GetUserByIdAsync(ToId);
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Content = FromUser.FirstName + " " + FromUser.LastName + " Send you friend request",
                HrefId = FromId,
                ApplicationUser = ToUser,
                ApplicationUserId = ToId
            };
            await _notificationRepo.AddNotificationAsync(notification);
            await _friendRepo.AddFriendRequest(FromId, ToId);
        }
    }
}
