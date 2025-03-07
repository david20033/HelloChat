using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Services.IServices
{
    public interface IHomeService
    {
        Task<List<ConversationsViewModel>> GetConversationsViewModel(string CurrentUserId);
        Task<List<IdentityUser>> GetIdentityUsersBySearchQuery(string query);
    }
}
