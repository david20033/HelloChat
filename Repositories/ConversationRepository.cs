using HelloChat.Data;
using HelloChat.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HelloChat.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly HelloChatDbContext _context;
        public ConversationRepository(HelloChatDbContext context)
        {
            _context = context;
        }

        public async Task AddConversationAsync(Conversation conversation)
        {
            await _context.Conversation.AddAsync(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetAnotherUserIdInConversationAsync(string UserId, Guid ConversationId)
        {
            var Conversation = await _context
               .Conversation
               .FirstAsync(c => c.Id == ConversationId);
            if (Conversation.User1Id == UserId)
            {
                return Conversation.User2Id;
            }
            else
            {
                return Conversation.User1Id;
            }
        }

        public async Task<Conversation?> GetConversationAsync(string user1Id, string user2Id)
        {
            return await _context.Conversation
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == user1Id && c.User2Id == user2Id) ||
                    (c.User1Id == user2Id && c.User2Id == user1Id));
        }

        public async Task<Conversation?> GetConversationByIdAsync(Guid Id)
        {
            return await _context.Conversation
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == Id);
        }

        public async Task<Guid> SetSeenToLastMessageAndReturnItsId(string UserId, Guid ConversationId)
        {
            var Conversation = await _context
                .Conversation
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == ConversationId);

            var lastMessage = Conversation
                .Messages
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            if (lastMessage?.From_id == UserId || lastMessage == null) return Guid.Empty;
            lastMessage.isSeen = true;
            lastMessage.SeenTime = DateTime.Now;
            await _context.SaveChangesAsync();
            return lastMessage.Id;
        }
    }
}
