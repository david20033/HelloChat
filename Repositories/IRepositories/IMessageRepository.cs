using HelloChat.Data;
using HelloChat.Enums;

namespace HelloChat.Repositories.IRepositories
{
    public interface IMessageRepository
    {
        Task AddMessageAsync(Message message);
        Task<Message?> GetLastMessageAsync(string sender, string receiver);
        Task<List<Message>> GetAllReceivedMessagesAsync(string sender, string receiver);
        Task SetMessageSeen(Guid messageId);
        Task<List<Message>> LoadMessages(Guid conversationId, int page);
        Task<List<Message>> LoadImages(Guid conversationId, int page);
        Task<Guid?> GetLastSeenMessageId(Guid conversationId, string receiverId);
        Task<bool> isLastMessageSeen(string userId, Guid conversationId);
        Task DeleteMessageContent(Guid messageId);
        Task SetLocalDeleted(Guid messageId);
        Task SetMessageReaction(Guid messageId, string fromId, string toId, MessageReaction reaction);
        Task<Message> GetMessageByIdAsync(Guid id);
    }
}
