using HelloChat.Data;
using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using NuGet.Protocol.Core.Types;

namespace HelloChat.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _convRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IUserRepository _userRepo;
        public ConversationService(IConversationRepository convRepo, 
            IMessageRepository messageRepo, 
            IUserRepository userRepo)
        {
            _convRepo = convRepo;
            _messageRepo = messageRepo;
            _userRepo = userRepo;
        }

        public async Task<string> GetAnotherUserId(Guid ConversationId, string UserId)
        {
            return await _convRepo.GetAnotherUserIdInConversationAsync(UserId, ConversationId);
        }

        public async Task<string> GetAnotherUserIdInConversationAsync(string UserId, Guid ConversationId)
        {
            return await _convRepo.GetAnotherUserIdInConversationAsync(UserId, ConversationId);
        }

        public async Task<HomeViewModel> GetConversationViewModel(Guid ConversationId, string SenderId)
        {
            var ReceiverId = await _convRepo.GetAnotherUserIdInConversationAsync(SenderId, ConversationId);
            var Receiver = await _userRepo.GetUserByIdAsync(ReceiverId);
            var Conversation = await _convRepo.GetConversationByIdAsync(ConversationId);
            if (Conversation == null) return new HomeViewModel { };
            var lastSeenMessage = Conversation
                .Messages
                .Where(m => m.isSeen == true && m.To_id == ReceiverId)
                .OrderByDescending(m => m.SeenTime)
                .FirstOrDefault();
            string active = await _userRepo.GetUserActiveString(ReceiverId);
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
            var ReceiverId = await _convRepo.GetAnotherUserIdInConversationAsync(SenderId, ConversationId);
            var Receiver = await _userRepo.GetUserByIdAsync(ReceiverId);
            var Conversation = await _convRepo.GetConversationByIdAsync(ConversationId);
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

        public async Task<Guid?> GetLastSeenMessageId(Guid ConversationId, string ReceiverId)
        {
            return await _messageRepo.GetLastSeenMessageId(ConversationId, ReceiverId);
        }

        public async Task<List<Message>> LoadImages(Guid ConversationId, int page)
        {
            return await _messageRepo.LoadImages(ConversationId, page);

        }

        public async Task<List<Message>> LoadMessages(Guid ConversationId, int page)
        {
            return await _messageRepo.LoadMessages(ConversationId, page);

        }

        public async Task<Guid> SetSeenToLastMessageAndReturnItsId(string UserId, Guid ConversationId)
        {
            var Conversation = await _convRepo.GetConversationByIdAsync(ConversationId);
            if (Conversation == null) return Guid.Empty;
            var lastMessage = Conversation
                .Messages
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            if (lastMessage?.From_id == UserId || lastMessage == null) return Guid.Empty;
            await _messageRepo.SetMessageSeen(lastMessage.Id);
            return lastMessage.Id;
        }
    }
}
