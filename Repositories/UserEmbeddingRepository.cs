using System.Data.Entity;
using HelloChat.Data;
using HelloChat.Repositories.IRepositories;

namespace HelloChat.Repositories
{
    public class UserEmbeddingRepository : IUserEmbeddingRepository
    {
        private readonly HelloChatDbContext _context;
        public UserEmbeddingRepository(HelloChatDbContext context)
        {
            _context = context;
        }
        public async Task<UserEmbedding?> GetUserByIdAsync(Guid userId)
        {
            return await _context.UserEmbedding.FindAsync(userId);
        }
        public async Task UpsertEmbeddingAsync(Guid userId, string embeddingJson)
        {
            var existing = await _context.UserEmbedding.FindAsync(userId);
            if (existing == null)
            {
                _context.UserEmbedding.Add(new UserEmbedding { Id = userId, EmbeddingJson = embeddingJson });
            }
            else
            {
                existing.EmbeddingJson = embeddingJson;
                _context.UserEmbedding.Update(existing);
            }

            await _context.SaveChangesAsync();
        }
        public async Task<List<UserEmbedding>> GetAllExceptUserAsync(Guid userId)
        {
            return await _context.UserEmbedding
                .Where(e => e.Id != userId)
                .ToListAsync();
        }
    }
}
