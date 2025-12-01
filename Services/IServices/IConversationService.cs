using HelloChat.Data;
using HelloChat.ViewModels;

namespace HelloChat.Services.IServices
{
    public interface IConversationService
    {
        Task<HomeViewModel> GetConversationViewModel(Guid conversationId, string senderId);
        Task<InfoViewModel> GetInfoViewModel(Guid conversationId, string senderId);
        Task<List<Message>> LoadMessages(Guid conversationId, int page);
        Task<List<Message>> LoadImages(Guid conversationId, int page);
        Task<Guid?> GetLastSeenMessageId(Guid conversationId, string receiverId);
        Task<Guid> SetSeenToLastMessageAndReturnItsId(string userId, Guid conversationId);
        Task<string> GetAnotherUserId(Guid conversationId, string userId);
        Task<string> GetAnotherUserIdInConversationAsync(string userId, Guid conversationId);
    }
}
