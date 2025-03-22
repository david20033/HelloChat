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
        public async Task<List<FriendsViewModel>> GetFriendsViewModel(string CurrentUserId)
        {
            return  await _repository.GetFriendsViewModelAsync(CurrentUserId);
        }
        public async Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query)
        {
            return await _repository.GetUsersBySearchQuery(query);
        }
        public async Task SendMessage(string FromId, string ToId, string Content)
        {
            await _repository.SendMessageAndReturnItsId(FromId,ToId,Content);
        }
        public async Task<HomeViewModel> GetConversationViewModel(Guid ConversationId, string SenderId)
        {
            return await _repository.GetConversationViewModel(ConversationId, SenderId);
        }
        public async Task<List<Message>> LoadMessages(Guid ConversationId, int page)
        {
            return await _repository.LoadMessages(ConversationId, page);
        
        }
        public async Task<Guid?> GetLastSeenMessageId(Guid ConversationId, string ReceiverId)
        {
            return await _repository.GetLastSeenMessageId(ConversationId, ReceiverId);
        }
        public async Task<string> GetAnotherUserId(Guid ConversationId, string UserId)
        {
            return await _repository.GetAnotherUserIdInConversationAsync(UserId,ConversationId);
        }
    }
}
