using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using NuGet.Protocol.Core.Types;

namespace HelloChat.Services
{
    public class MessageService : IMessageService
    {
        private readonly IConversationRepository _convRepo;
        private readonly IMessageRepository _messRepo;
        public MessageService(IConversationRepository convRepo, IMessageRepository messRepo)
        {
            _convRepo = convRepo;
            _messRepo = messRepo;
        }
        public async Task DeleteMessageContent(Guid MessageId)
        {
            await _messRepo.DeleteMessageContent(MessageId);
        }



        public async Task<bool> IsLastMessageSeen(string userId, Guid ConversationId)
        {
            var Conversation = await _convRepo.GetConversationByIdAsync(ConversationId);
            if (Conversation == null) return false;
            var lastMessage = Conversation
                .Messages
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            if (lastMessage == null) return false;
            return lastMessage.isSeen;
        }

        public async Task<Guid> SendAudioAndReturnItsId(string FromId, string ToId, string base64Audio)
        {
            if (base64Audio == null) return Guid.Empty;
            byte[] audioBytes = Convert.FromBase64String(base64Audio);
            var Conversation = await _convRepo.GetConversationAsync(FromId, ToId);
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
            await _messRepo.AddMessageAsync(Message);
            return Message.Id;
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
            var Conversation = await _convRepo.GetConversationAsync(FromId, ToId);
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
            await _messRepo.AddMessageAsync(Message);
            return (Message.Id, imageUrl);
        }

        public async Task<Guid> SendMessageAndReturnItsId(string FromId, string ToId, string Content)
        {
            var Conversation = await _convRepo.GetConversationAsync(FromId, ToId);
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
            await _messRepo.AddMessageAsync(Message);
            return Message.Id;
        }
        public async Task SetLocalDelete(Guid MessageId)
        {
            await _messRepo.SetLocalDeleted(MessageId);
        }

        public async Task SetMessageReaction(Guid MessageId, string From_Id, string To_Id, MessageReaction reaction)
        {
            await _messRepo.SetMessageReaction(MessageId, From_Id, To_Id, reaction);
        }
    }
}
