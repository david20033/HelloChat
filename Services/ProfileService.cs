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
            await _appRepository.DeleteFriend(FromId, ToId);
        }
        public async Task DeleteFriendRequest(string FromId, string ToId)
        {
            await _appRepository.DeleteFriendRequest(FromId, ToId);
        }
        public async Task AcceptFriendRequest(string FromId, string ToId)
        {
            await _appRepository.AcceptFriendRequest(FromId, ToId);
        }
    }
}
