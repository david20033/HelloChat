using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;

namespace HelloChat.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IAppRepository _appRepository;

        public ProfileService(IAppRepository appRepository) 
        {
            _appRepository = appRepository;
        }
        public async Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId)
        {
            
            return await _appRepository.GetProfileViewModelById(ProfileUserId, CurrentUserId);
        }
        public async Task SendFriendRequest(string FromId, string ToId)
        {
            await _appRepository.AddFriendRequest(FromId, ToId);
        }
        public async Task DeleteFriend(string FromId, string ToId)
        {

        }
    }
}
