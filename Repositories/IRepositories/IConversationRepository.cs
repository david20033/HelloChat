using HelloChat.Data;

namespace HelloChat.Repositories.IRepositories
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetConversationByIdAsync(Guid id);
        Task AddConversationAsync(Conversation conversation);
        Task<Conversation?> GetConversationAsync(string user1Id, string user2Id);
        Task<string> GetAnotherUserIdInConversationAsync(string userId, Guid conversationId);
        Task<Guid> SetSeenToLastMessageAndReturnItsId(string userId, Guid conversationId);
    }
}
