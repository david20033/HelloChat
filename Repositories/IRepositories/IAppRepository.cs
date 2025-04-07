using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Repositories.IRepositories
{
    public interface IAppRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(string UserId);
        Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(string UserId);
        Task<List<Message>> GetAllReceivedMessagesAsync(string Sender, string Receiver);
        Task<Message?> GetLastMessageAsync(string Sender, string Receiver);
        Task<Conversation?> GetConversationByIdAsync(Guid Id);
        Task AddConversationAsync(Conversation conversation);
        Task<Conversation?> GetConversationAsync(string User1Id, string User2Id);
        Task<bool> IsFriendRequestExistsAsync(string SenderId, string ReceiverId);
        Task<bool> IsFriendRequestAccepted(string User1Id, string User2Id);
        Task SetMessageSeen(Guid MessageId);
      
        Task<List<Message>> LoadMessages(Guid ConversationId, int page);
        Task<List<Message>> LoadImages(Guid ConversationId, int page);
        Task<Guid?> GetLastSeenMessageId(Guid ConversationId, string ReceiverId);
        Task<List<ApplicationUser>> GetUsersBySearchQuery(string query);
        Task AddFriendRequest(string FromId, string ToId);
        Task DeleteFriend(string FromId, string ToId);
        Task DeleteFriendRequest(string FromId, string ToId);

        Task AcceptFriendRequest(string FromId, string ToId);

        Task AddMessageAsync(Message message);
        Task<List<string>> GetUserFriendIds(string UserId);
        Task<Guid> SetSeenToLastMessageAndReturnItsId(string UserId, Guid ConversationId);
        Task<string> GetAnotherUserIdInConversationAsync(string UserId, Guid ConversationId);
        Task<bool> isLastMessageSeen(string UserId, Guid ConversationId);
        Task DeleteMessageContent(Guid MessageId);

        Task SetLocalDeleted(Guid MessageId);
        Task SetMessageReaction(Guid MessageId, string From_Id, string To_Id, MessageReaction reaction);

        Task SetUserActive(string UserId);
        Task SetUserExitActive(string UserId);
        Task<string> GetUserActiveString(string UserId);
        Task UpdateUserPicturePath(string UserId, string PicturePath);
        Task EditProfile(EditProfileViewModel model);
        Task<Message> GetMessageByIdAsync(Guid Id);
        Task AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotifications(string UserId);
        Task RemoveNotificationByHref(string href);
    }
}
