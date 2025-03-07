using HelloChat.ViewModels;

namespace HelloChat.Services.IServices
{
    public interface IProfileService
    {
        Task<ProfileViewModel> GetProfileViewModelById(string id);
    }
}
