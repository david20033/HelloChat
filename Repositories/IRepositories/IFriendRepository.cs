using HelloChat.Data;

namespace HelloChat.Repositories.IRepositories
{
    public interface IFriendRepository
    {
        Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(string userId);
        Task AddFriendRequest(string fromId, string toId);
        Task DeleteFriend(string fromId, string toId);
        Task DeleteFriendRequest(string fromId, string toId);
        Task AcceptFriendRequest(string fromId, string toId);
        Task<bool> IsFriendRequestExistsAsync(string senderId, string receiverId);
        Task<bool> IsFriendRequestAccepted(string user1Id, string user2Id);
        Task<List<string>> GetUserFriendIds(string userId);
        Task<int> CountMutualFriendsAsync(Guid userId, Guid otherUserId);
        Task<List<Guid>> GetFriendsAsync(Guid userId);
        Task<List<Guid>> GetMutualFriendsAsync(Guid userId, Guid otherUserId);
        Task<bool> AreFriendsAsync(Guid userId, Guid otherId);
        Task<string> GetCommonInterestsAsync(string User1Id, string User2Id);
    }
}
