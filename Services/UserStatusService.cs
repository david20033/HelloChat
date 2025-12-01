using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;

namespace HelloChat.Services
{
    public class UserStatusService : IUserStatusService
    {
        private readonly IUserRepository _userRepo;
        public UserStatusService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public async Task<string> GetUserActiveString(string userId)
        {
            return await _userRepo.GetUserActiveString(userId);
        }

        public async Task SetUserActive(string userId)
        {
             await _userRepo.SetUserActive(userId);
        }

        public async Task SetUserExitActive(string userId)
        {
            await _userRepo.SetUserExitActive(userId);
        }
    }
}
