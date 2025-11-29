using System.Speech.Synthesis;
using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelloChat.Services
{
    public class HomeService : IHomeService
    {
        private readonly IAppRepository _repository;
        private readonly IOpenAiService _openAi;

        public HomeService(IAppRepository repository,IOpenAiService OpenAi) 
        {
            _repository = repository;
            _openAi = OpenAi;
        }
        public async Task<List<FriendsViewModel>> GetFriendsViewModelAsync(string CurrentUserId)
        {
            var Friendships = await _repository.GetUserFriendshipsAsync(CurrentUserId);
            List<FriendsViewModel> FriendsList = [];
            foreach (var f in Friendships)
            {
                ApplicationUser user = null;
                if (f.User1Id == CurrentUserId)
                {
                    user = await _repository.GetUserByIdAsync(f.User2Id);
                }
                else
                {
                    user = await _repository.GetUserByIdAsync(f.User1Id);
                }
                var Messages = await _repository.GetAllReceivedMessagesAsync(CurrentUserId, user.Id);
                var LastMessage = await _repository.GetLastMessageAsync(CurrentUserId, user.Id);
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
                if (await _repository.GetConversationAsync(CurrentUserId, user.Id) == null)
                {
                    var el = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        User1Id = CurrentUserId,
                        User2Id = user.Id,
                    };
                    FriendModel.ConversationId = el.Id;
                    await _repository.AddConversationAsync(el);
                }
                else if (FriendModel.ConversationId == null)
                {
                    var el = await _repository.GetConversationAsync(CurrentUserId, user.Id);
                    FriendModel.ConversationId = el?.Id;
                }
                FriendsList.Add(FriendModel);
            }
            return FriendsList;
        }
        public async Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query)
        {
            return await _repository.GetUsersBySearchQuery(query);
        }
        public async Task<Guid> SendMessageAndReturnItsId(string FromId, string ToId, string Content)
        {
            var Conversation = await _repository.GetConversationAsync(FromId, ToId);
            var Message = new Message
            {
                Id = Guid.NewGuid(),
                Conversation = Conversation,
                ConversationId = Conversation.Id,
                From_id = FromId,
                To_id = ToId,
                Content = Content,
                CreatedDate = DateTime.Now,
                isSeen = false,
                SeenTime = null,
                ImageUrl = null,
            };
            await _repository.AddMessageAsync(Message);
            return Message.Id;
        }
        public async Task<HomeViewModel> GetConversationViewModel(Guid ConversationId, string SenderId)
        {
            var ReceiverId = await _repository.GetAnotherUserIdInConversationAsync(SenderId, ConversationId);
            var Receiver = await _repository.GetUserByIdAsync(ReceiverId);
            var Conversation = await _repository.GetConversationByIdAsync(ConversationId);
            if (Conversation == null) return new HomeViewModel { };
            var lastSeenMessage = Conversation
                .Messages
                .Where(m => m.isSeen == true && m.To_id == ReceiverId)
                .OrderByDescending(m => m.SeenTime)
                .FirstOrDefault();
            string active = await _repository.GetUserActiveString(ReceiverId);
            return new HomeViewModel
            {
                CurrentConversationId = ConversationId,
                Messages = Conversation.Messages
                .OrderByDescending(m => m.CreatedDate)
                .Take(10)
                .OrderBy(m => m.CreatedDate)
                .ToList(),
                SenderId = SenderId,
                ReceiverId = ReceiverId,
                Name = $"{Receiver.FirstName} {Receiver.LastName}",
                ProfilePicturePath = Receiver.ProfilePicturePath,
                LastSeenMessageId = lastSeenMessage?.Id,
                ActiveString = active,
            };
        }
        public async Task<InfoViewModel> GetInfoViewModel(Guid ConversationId, string SenderId)
        {
            var ReceiverId = await _repository.GetAnotherUserIdInConversationAsync(SenderId, ConversationId);
            var Receiver = await _repository.GetUserByIdAsync(ReceiverId);
            var Conversation = await _repository.GetConversationByIdAsync(ConversationId);
            if (Conversation == null) return new InfoViewModel { };
            var ImageUrls = Conversation.Messages
                .Where(m => m.ImageUrl != null)
                .Take(10)
                .Select(m => m.ImageUrl)
                .ToList();
            return new InfoViewModel
            {
                Name = Receiver.FirstName + " " + Receiver.LastName,
                ProfileImagePath = Receiver.ProfilePicturePath,
                ImagesUrls = ImageUrls,
            };
        }
        public async Task<List<Message>> LoadMessages(Guid ConversationId, int page)
        {
            return await _repository.LoadMessages(ConversationId, page);
        
        }
        public async Task<List<Message>> LoadImages(Guid ConversationId, int page)
        {
            return await _repository.LoadImages(ConversationId, page);

        }
        public async Task<Guid?> GetLastSeenMessageId(Guid ConversationId, string ReceiverId)
        {
            return await _repository.GetLastSeenMessageId(ConversationId, ReceiverId);
        }
        public async Task<string> GetAnotherUserId(Guid ConversationId, string UserId)
        {
            return await _repository.GetAnotherUserIdInConversationAsync(UserId,ConversationId);
        }
        public async Task<Guid> SetSeenToLastMessageAndReturnItsId(string UserId, Guid ConversationId)
        {
            var Conversation = await _repository.GetConversationByIdAsync(ConversationId);
            if (Conversation == null) return Guid.Empty;
            var lastMessage = Conversation
                .Messages
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            if (lastMessage?.From_id == UserId || lastMessage == null) return Guid.Empty;
            await _repository.SetMessageSeen(lastMessage.Id);
            return lastMessage.Id;
        }
        public async Task EditMessage(Message message)
        {
            if (message == null) return;
            
        }
        public async Task<(Guid, string)> SendImageAndReturnItsIdAndUrl(string FromId, string ToId, string imageName, string base64Image)
        {
            if (imageName == null || base64Image == null) return (Guid.Empty, "");
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedImages", "ChatImages");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageName;
            string filePath = Path.Combine(folderPath, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, imageBytes);
            string imageUrl = $"/UploadedImages/ChatImages/{uniqueFileName}";
            var Conversation = await _repository.GetConversationAsync(FromId, ToId);
            var Message = new Message
            {
                Id = Guid.NewGuid(),
                Conversation = Conversation,
                ConversationId = Conversation.Id,
                From_id = FromId,
                To_id = ToId,
                Content = "Photo message",
                CreatedDate = DateTime.Now,
                isSeen = false,
                SeenTime = null,
                ImageUrl = imageUrl,
            };
            await _repository.AddMessageAsync(Message);
            return (Message.Id, imageUrl);
        }
        public async Task<Guid> SendAudioAndReturnItsId(string FromId, string ToId, string base64Audio)
        {
            if (base64Audio == null) return Guid.Empty;
            byte[] audioBytes = Convert.FromBase64String(base64Audio);
            var Conversation = await _repository.GetConversationAsync(FromId, ToId);
            var Message = new Message
            {
                Id = Guid.NewGuid(),
                Conversation = Conversation,
                ConversationId = Conversation.Id,
                From_id = FromId,
                To_id = ToId,
                Content = "Audio message",
                CreatedDate = DateTime.Now,
                isSeen = false,
                SeenTime = null,
                AudioFile = audioBytes,
            };
            await _repository.AddMessageAsync(Message);
            return Message.Id;
        }
        public async Task DeleteMessageContent(Guid MessageId)
        {
            await _repository.DeleteMessageContent(MessageId);
        }
        public async Task SetLocalDelete(Guid MessageId)
        {
            await _repository.SetLocalDeleted(MessageId);
        }
        public async Task<string> GetAnotherUserIdInConversationAsync(string UserId, Guid ConversationId)
        {
            return await _repository.GetAnotherUserIdInConversationAsync(UserId, ConversationId);
        }
        public async Task<string> GetUserActiveString(string UserId)
        {
            string active = "";
            var User = await _repository.GetUserByIdAsync(UserId);
            if (User != null && User.isActive)
            {
                active = "Active Now";
            }
            else if (User != null)
            {
                DateTime now = DateTime.Now;
                DateTime lastActive = User.LastTimeActive;
                TimeSpan difference = now - lastActive;
                if (difference < TimeSpan.FromMinutes(5))
                {
                    active = "Last Active: Just Now";
                }
                else if (difference >= TimeSpan.FromMinutes(5) && difference <= TimeSpan.FromMinutes(59))
                {
                    active = $"Last Active: {difference.Minutes.ToString()} Minute/s ago";
                }
                else if (difference >= TimeSpan.FromHours(1) && difference <= TimeSpan.FromHours(23))
                {
                    active = $"Last Active: {difference.Hours.ToString()} Hour/s ago";
                }
                else if (difference >= TimeSpan.FromDays(1))
                {
                    active = $"Last Active: {difference.Days.ToString()} Day/s ago";
                }
            }
            return active;
        }
        public async Task SetMessageReaction(Guid MessageId, string From_Id, string To_Id, MessageReaction reaction)
        {
            await _repository.SetMessageReaction(MessageId, From_Id, To_Id, reaction);
        }
        public async Task<bool> isLastMessageSeen(string UserId, Guid ConversationId)
        {
            var Conversation = await _repository.GetConversationByIdAsync(ConversationId);
            if(Conversation == null) return false;
            var lastMessage = Conversation
                .Messages
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            if (lastMessage == null) return false;
            return lastMessage.isSeen;
        }
        public async Task SetUserActive(string UserId)
        {
            await _repository.SetUserActive(UserId);
        }
        public async Task SetUserExitActive(string UserId)
        {
            await _repository.SetUserExitActive(UserId);
        }
        public async Task<List<string>> GetUserFriendIds(string UserId)
        {

            var Friendships = await _repository.GetUserFriendshipsAsync(UserId);
            List<string> FriendsList = [];
            foreach (var f in Friendships)
            {
                ApplicationUser us = null;
                if (f.User1Id == UserId)
                {
                    us = await _repository.GetUserByIdAsync(f.User2Id);
                }
                else
                {
                    us = await _repository.GetUserByIdAsync(f.User1Id);
                }
                FriendsList.Add(us.Id);
            }
            return FriendsList;
        }

    }
}
