using HelloChat.ViewModels;

namespace HelloChat.Services.IServices
{
    public interface IHomeService
    {
        Task<List<ConversationsViewModel>> GetConversationsViewModel(Guid CurrentUserId);
    }
}
