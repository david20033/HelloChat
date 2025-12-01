using HelloChat.Data;
using HelloChat.ViewModels;

namespace HelloChat.Services.IServices
{
    public interface IProfileService
    {
        Task RemoveNotificationAsync(string hrefId);
        Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId);

        Task<EditProfileViewModel> GetEditProfileViewModel(string UserId);
        Task<(string, string)> TryToEditProfile(EditProfileViewModel model);
        Task<string> GetUserNameById(string id);
        Task<List<Notification>> GetNotificationsAsync(string UserId);
    }
}
