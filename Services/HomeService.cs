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
        public async Task<HomeViewModel> GetConversationsViewModel(string CurrentUserId, string User2Id)
        {
            return  await _repository.GetHomeViewModelAsync(CurrentUserId,User2Id);
        }
        public async Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query)
        {
            return await _repository.GetUsersBySearchQuery(query);
        }
        public async Task SendMessage(string FromId, string ToId, string Content)
        {
            await _repository.SendMessage(FromId,ToId,Content);
        }
    }
}
