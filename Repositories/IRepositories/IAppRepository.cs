using HelloChat.Data;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Repositories.IRepositories
{
    public interface IAppRepository
    {
        Task<List<ConversationsViewModel>> GetConversationsAsync(string userGuid);
        Task<List<ApplicationUser>> GetUsersBySearchQuery(string query);
        Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId);
        Task AddFriendRequest(string FromId, string ToId);
        Task DeleteFriend(string FromId, string ToId);
        Task DeleteFriendRequest(string FromId, string ToId);

        Task AcceptFriendRequest(string FromId, string ToId);
    }
}
