using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.Models;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Services.IServices
{
    public interface IHomeService
    {
        Task<List<FriendsViewModel>> GetFriendsViewModelAsync(string CurrentUserId);
        Task<List<ApplicationUser>> GetIdentityUsersBySearchQuery(string query);
        Task<Guid> SendMessageAndReturnItsId(string FromId, string ToId, string Content);
        Task<Guid> SendAudioAndReturnItsId(string FromId, string ToId, string base64Audio);
        Task<HomeViewModel> GetConversationViewModel(Guid ConversationId, string SenderId);
        Task<InfoViewModel> GetInfoViewModel(Guid ConversationId, string SenderId);

        Task<List<Message>> LoadMessages(Guid ConversationId, int page);
        Task<List<Message>> LoadImages(Guid ConversationId, int page);
        Task<Guid?> GetLastSeenMessageId(Guid ConversationId, string ReceiverId);
        Task<string> GetAnotherUserId(Guid ConversationId, string UserId);
        Task<Guid> SetSeenToLastMessageAndReturnItsId(string UserId, Guid ConversationId);
        Task<(Guid, string)> SendImageAndReturnItsIdAndUrl(string FromId, string ToId, string imageName, string base64Image);
         Task DeleteMessageContent(Guid MessageId);
        Task SetLocalDelete(Guid MessageId);
        Task<string> GetAnotherUserIdInConversationAsync(string UserId, Guid ConversationId);
        Task<string> GetUserActiveString(string UserId);
        Task SetMessageReaction(Guid MessageId, string From_Id, string To_Id, MessageReaction reaction);
        Task<bool> isLastMessageSeen(string UserId, Guid ConversationId);
        Task SetUserActive(string UserId);
        Task SetUserExitActive(string UserId);
        Task<List<string>> GetUserFriendIds(string UserId);
        Task <List<RecommendedFriendViewModel>> MapFromRecommendationResultToRecommendedFriendViewModel(List<RecommendationResult> recommendations);
    }
}
