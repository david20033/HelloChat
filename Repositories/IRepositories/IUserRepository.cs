using HelloChat.Data;
using HelloChat.ViewModels;

namespace HelloChat.Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task SetUserActive(string id);
        Task SetUserExitActive(string id);
        Task<string> GetUserActiveString(string id);
        Task UpdateUserPicturePath(string userId, string picturePath);
        Task EditProfile(EditProfileViewModel model);
        Task<List<ApplicationUser>> GetUsersBySearchQuery(string query);
    }
}
