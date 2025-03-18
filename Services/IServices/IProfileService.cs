using HelloChat.ViewModels;

namespace HelloChat.Services.IServices
{
    public interface IProfileService
    {
        Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId);
        Task SendFriendRequest(string FromId, string ToId);
        Task DeleteFriend(string FromId, string ToId);
        Task DeleteFriendRequest(string FromId, string ToId);
        Task AcceptFriendRequest(string FromId, string ToId);
        Task<EditProfileViewModel> GetEditProfileViewModel(string UserId);
        Task<(string, string)> TryToEditProfile(EditProfileViewModel model);
    }
}
