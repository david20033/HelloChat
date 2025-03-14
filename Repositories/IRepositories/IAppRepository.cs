using HelloChat.Data;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Repositories.IRepositories
{
    public interface IAppRepository
    {
        Task<HomeViewModel> GetHomeViewModelAsync(string CurrentUserId, string User2Id);
        Task<List<ApplicationUser>> GetUsersBySearchQuery(string query);
        Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId);
        Task AddFriendRequest(string FromId, string ToId);
        Task DeleteFriend(string FromId, string ToId);
        Task DeleteFriendRequest(string FromId, string ToId);

        Task AcceptFriendRequest(string FromId, string ToId);

        Task<Guid> SendMessageAndReturnItsId(string FromId, string ToId, string Content);
        Task<List<string>> GetUserFriendIds(string UserId);
        Task<Guid> SetSeenToLastMessageAndReturnItsId(string UserId, Guid ConversationId);
        Task<string> GetAnotherUserIdInConversationAsync(string UserId, Guid ConversationId);
        Task<bool> isLastMessageSeen(string UserId, Guid ConversationId);
        Task DeleteMessageContent(Guid MessageId);

        Task SetLocalDeleted(Guid MessageId);
    }
}
