using HelloChat.ViewModels;

namespace HelloChat.Services.IServices
{
    public interface IProfileService
    {
        Task<ProfileViewModel> GetProfileViewModelById(string id);
        Task SendFriendRequest(string FromId, string ToId);
    }
}
