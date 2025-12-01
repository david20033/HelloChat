using HelloChat.Data;
using HelloChat.Models;
using HelloChat.ViewModels;

namespace HelloChat.Services.IServices
{
    public interface IFriendService
    {
        Task<List<FriendsViewModel>> GetFriendsViewModelAsync(string currentUserId);
        Task<List<string>> GetUserFriendIds(string userId);
        Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query);
        Task<List<RecommendedFriendViewModel>> MapFromRecommendationResultToRecommendedFriendViewModel(
            List<RecommendationResult> recommendations);

        Task SendFriendRequest(string FromId, string ToId);
        Task DeleteFriend(string FromId, string ToId);
        Task DeleteFriendRequest(string FromId, string ToId);
        Task AcceptFriendRequest(string FromId, string ToId);
    }
}
