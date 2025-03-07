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
        public async Task<ProfileViewModel> GetProfileViewModelById(string id)
        {
            return await _appRepository.GetProfileViewModelById(id);
        }
    }
}
