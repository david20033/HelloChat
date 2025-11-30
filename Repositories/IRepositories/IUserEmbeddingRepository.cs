using HelloChat.Data;

namespace HelloChat.Repositories.IRepositories
{
    public interface IUserEmbeddingRepository
    {
        Task<UserEmbedding?> GetUserByIdAsync(Guid userId);
        Task InsertEmbeddingAsync(Guid userId, string embeddingJson);
        Task<List<UserEmbedding>> GetAllExceptUserAsync(Guid userId);
    }
}
