using HelloChat.Data;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Services.IServices
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetConversationsViewModel(string CurrentUserId, string User2Id);
        Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query);
        Task SendMessage(string FromId, string ToId, string Content);
    }
}
