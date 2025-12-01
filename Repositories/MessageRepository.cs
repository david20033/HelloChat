using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HelloChat.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly HelloChatDbContext _context;
        public MessageRepository(HelloChatDbContext context)
        {
            _context = context;
        }

        public async Task AddMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMessageContent(Guid MessageId)
        {
            var message = await _context.Messages.Where(m => m.Id == MessageId).FirstOrDefaultAsync();
            if (message == null) return;
            message.Content = "Message Removed";
            message.isDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Message>> GetAllReceivedMessagesAsync(string Sender, string Receiver)
        {
            return await _context.Messages.Where(m => m.To_id == Sender && m.From_id == Receiver).ToListAsync();
        }

        public async Task<Message?> GetLastMessageAsync(string Sender, string Receiver)
        {
            return await _context.Messages
                .Where(m => (m.From_id == Sender && m.To_id == Receiver) || (m.From_id == Receiver && m.To_id == Sender))
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<Guid?> GetLastSeenMessageId(Guid ConversationId, string ReceiverId)
        {
            var Conversation = await _context
                .Conversation
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == ConversationId);
            var lastSeenMessage = Conversation
                .Messages
                .Where(m => m.isSeen == true && m.To_id == ReceiverId)
                .OrderByDescending(m => m.SeenTime)
                .FirstOrDefault();
            return lastSeenMessage?.Id;
        }

        public async Task<Message> GetMessageByIdAsync(Guid Id)
        {
            return await _context.Messages.FirstAsync(m => m.Id == Id);
        }
        public async Task<bool> isLastMessageSeen(string UserId, Guid ConversationId)
        {
            var Conversation = await _context
                .Conversation
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == ConversationId);
            var lastMessage = Conversation
                .Messages
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            if (lastMessage == null) return false;
            return lastMessage.isSeen;
        }

        public async Task<List<Message>> LoadImages(Guid ConversationId, int page)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == ConversationId && m.ImageUrl != null)
                .OrderByDescending(m => m.CreatedDate)
                .Skip((page - 1) * 10)
                .Take(10)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Message>> LoadMessages(Guid ConversationId, int page)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == ConversationId)
                .OrderByDescending(m => m.CreatedDate)
                .Skip((page - 1) * 10)
                .Take(10)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task SetLocalDeleted(Guid MessageId)
        {
            var message = await _context.Messages.Where(m => m.Id == MessageId).FirstOrDefaultAsync();
            if (message == null) return;
            message.isLocalDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task SetMessageReaction(Guid MessageId, string From_Id, string To_Id, MessageReaction reaction)
        {
            var Message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == MessageId);
            if (Message == null) return;
            if (Message.From_id == From_Id && Message.To_id == To_Id)
            {
                Message.ReactionFromSender = reaction;
            }
            else if (Message.To_id == From_Id && Message.From_id == To_Id)
            {
                Message.ReactionFromReceiver = reaction;
            }
            await _context.SaveChangesAsync();
        }

        public async Task SetMessageSeen(Guid MessageId)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == MessageId);
            if (message == null) return;
            message.isSeen = true;
            message.SeenTime = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
