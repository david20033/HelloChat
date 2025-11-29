using HelloChat.Repositories;
using HelloChat.Services.IServices;

namespace HelloChat.Services
{
    public class FriendRecommendationService
    {
        private readonly UserEmbeddingRepository _repo;
        private readonly OpenAiService _openAi;
        public FriendRecommendationService(UserEmbeddingRepository repo, OpenAiService openAi)
        {
            _repo = repo;
            _openAi = openAi;
        }
        private double CosineSimilarity(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
        {
            double dot = 0, normA = 0, normB = 0;
            var aSpan = a.Span;
            var bSpan = b.Span;

            for (int i = 0; i < a.Length; i++)
            {
                dot += aSpan[i] * bSpan[i];
                normA += aSpan[i] * aSpan[i];
                normB += bSpan[i] * bSpan[i];
            }

            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        public async Task<List<(Guid userId, double score)>> RecommendFriendsAsync(Guid currentUserId)
        {
            var currentUserEmbedding = await _repo.GetUserByIdAsync(currentUserId);
            if (currentUserEmbedding == null) return new List<(Guid, double)>();

            var currentVec = _openAi.DeserializeEmbedding(currentUserEmbedding.EmbeddingJson);
            var others = await _repo.GetAllExceptUserAsync(currentUserId);

            var results = others
                .Select(o => (userId: o.Id, score: CosineSimilarity(currentVec, _openAi.DeserializeEmbedding(o.EmbeddingJson))))
                .OrderByDescending(x => x.score)
                .Take(5)
                .ToList();

            return results;
        }
    }
}
