using HelloChat.Data;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Services.IServices
{
    public interface IHomeService
    {
        Task<List<FriendsViewModel>> GetFriendsViewModel(string CurrentUserId);
        Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query);
        Task SendMessage(string FromId, string ToId, string Content);
        Task<HomeViewModel> GetConversationViewModel(Guid ConversationId, string SenderId);
    }
}
