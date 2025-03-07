using HelloChat.Data;
using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Services
{
    public class HomeService : IHomeService
    {
        private readonly IAppRepository _repository;

        public HomeService(IAppRepository repository) 
        {
            _repository = repository;
        }
        public async Task<List<ConversationsViewModel>> GetConversationsViewModel(string CurrentUserId)
        {
            return  await _repository.GetConversationsAsync(CurrentUserId);
        }
        public async Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query)
        {
            return await _repository.GetUsersBySearchQuery(query);
        }
    }
}
