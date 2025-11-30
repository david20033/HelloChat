using HelloChat.Models;

namespace HelloChat.Services.IServices
{
    public interface IFriendRecommendationService
    {
        Task<List<RecommendationResult>> RecommendFriendsAsync(Guid userId, int take = 10);
    }
}
