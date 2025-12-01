using HelloChat.Enums;

namespace HelloChat.Services.IServices
{
    public interface IMessageService
    {
        Task<Guid> SendMessageAndReturnItsId(string fromId, string toId, string content);
        Task<Guid> SendAudioAndReturnItsId(string fromId, string toId, string base64Audio);
        Task<(Guid, string)> SendImageAndReturnItsIdAndUrl(string fromId, string toId, string imageName, string base64Image);

        Task DeleteMessageContent(Guid messageId);
        Task SetLocalDelete(Guid messageId);
        Task SetMessageReaction(Guid messageId, string fromId, string toId, MessageReaction reaction);

        Task<bool> IsLastMessageSeen(string userId, Guid conversationId);
    }
}
