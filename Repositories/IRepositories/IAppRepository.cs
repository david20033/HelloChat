using HelloChat.Data;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Repositories.IRepositories
{
    public interface IAppRepository
    {
        Task<List<ConversationsViewModel>> GetConversationsAsync(string userGuid);
        Task<List<ApplicationUser>> GetUsersBySearchQuery(string query);
    }
}
