using HelloChat.Data;
using HelloChat.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HelloChat.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly HelloChatDbContext _context;
        public FriendRepository(HelloChatDbContext context)
        {
            _context = context;
        }

        public async Task AcceptFriendRequest(string FromId, string ToId)
        {
            if (FromId == ToId || FromId.IsNullOrEmpty() || ToId.IsNullOrEmpty()) return;
            var Request = await _context.FriendRequest
                .FirstOrDefaultAsync(fr => fr.RequesterId == ToId && fr.ReceiverId == FromId);
            if (Request == null)
            {
                return;
            }
            Request.isAccepted = true;
            var Frienship = new Friendship
            {
                Id = Guid.NewGuid(),
                User1Id = FromId,
                User2Id = ToId,
                FriendShipDate = DateTime.Now,
            };
            await _context.Friendship.AddAsync(Frienship);
            await _context.SaveChangesAsync();
        }

        public async Task AddFriendRequest(string FromId, string ToId)
        {
            if (FromId == ToId || FromId.IsNullOrEmpty() || ToId.IsNullOrEmpty()) return;

            await _context.FriendRequest.AddAsync(new FriendRequest
            {
                Id = Guid.NewGuid(),
                RequesterId = FromId,
                ReceiverId = ToId,
                RequestDate = DateTime.Now,
            });
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AreFriendsAsync(Guid userId, Guid otherId)
        {
            return await _context.Friendship
                .Where(f => (f.User1Id == userId.ToString() && f.User2Id == otherId.ToString()) ||
                            (f.User2Id == userId.ToString() && f.User1Id == otherId.ToString()))
                .AnyAsync();
        }

        public async Task<int> CountMutualFriendsAsync(Guid userId, Guid otherUserId)
        {
            string userIdStr = userId.ToString();
            string otherUserIdStr = otherUserId.ToString();

            var userFriends = _context.Friendship
                .Where(f => f.User1Id == userIdStr || f.User2Id == userIdStr)
                .Select(f => f.User1Id == userIdStr ? f.User2Id : f.User1Id);

            var otherUserFriends = _context.Friendship
                .Where(f => f.User1Id == otherUserIdStr || f.User2Id == otherUserIdStr)
                .Select(f => f.User1Id == otherUserIdStr ? f.User2Id : f.User1Id);


            var mutualFriendsCount = await userFriends
                .Intersect(otherUserFriends)
                .CountAsync();

            return mutualFriendsCount;
        }

        public async Task DeleteFriend(string FromId, string ToId)
        {
            if (FromId == ToId || FromId.IsNullOrEmpty() || ToId.IsNullOrEmpty()) return;
            var FriendShip = await _context.Friendship
                .FirstOrDefaultAsync(fr => (fr.User1Id == FromId && fr.User2Id == ToId)
                || (fr.User2Id == FromId && fr.User1Id == ToId));
            var FriendRequest = await _context.FriendRequest
                .FirstOrDefaultAsync(fr => (fr.ReceiverId == FromId && fr.RequesterId == ToId)
                || (fr.RequesterId == FromId && fr.ReceiverId == ToId));

            if (FriendShip != null && FriendRequest != null)
            {
                _context.Friendship.Remove(FriendShip);
                _context.FriendRequest.Remove(FriendRequest);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteFriendRequest(string FromId, string ToId)
        {
            if (FromId == ToId || FromId.IsNullOrEmpty() || ToId.IsNullOrEmpty()) return;
            var Request = await _context.FriendRequest
                .FirstOrDefaultAsync(fr => fr.RequesterId == FromId && fr.ReceiverId == ToId);
            if (Request != null)
            {
                _context.FriendRequest.Remove(Request);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Guid>> GetFriendsAsync(Guid userId)
        {
            return await _context.Friendship
                .Where(f => f.User1Id == userId.ToString() || f.User2Id == userId.ToString())
                .Select(f => f.User1Id == userId.ToString() ? Guid.Parse(f.User2Id) : Guid.Parse(f.User1Id))
                .ToListAsync();
        }
        public async Task<List<Guid>> GetMutualFriendsAsync(Guid userId, Guid otherUserId)
        {
            return await _context.Friendship
                .Where(f => (f.User1Id == userId.ToString() || f.User2Id == userId.ToString()) &&
                            (f.User1Id == otherUserId.ToString() || f.User2Id == otherUserId.ToString()))
                .Select(f => f.User1Id == userId.ToString() ? Guid.Parse(f.User2Id) : Guid.Parse(f.User1Id))
                .ToListAsync();

        }

        public async Task<List<string>> GetUserFriendIds(string UserId)
        {
            var user = await _context.Users
                .Include(u => u.FriendshipsInitiated)
                .Include(u => u.FriendshipsReceived)
                .FirstAsync(u => u.Id == UserId);
            var Friendships = user.FriendshipsInitiated.Concat(user.FriendshipsReceived);
            List<string> FriendsList = [];
            foreach (var f in Friendships)
            {
                ApplicationUser? us = null;
                if (f.User1Id == UserId)
                {
                    us = await _context.Users.Where(u => u.Id == f.User2Id).FirstAsync();
                }
                else
                {
                    us = await _context.Users.Where(u => u.Id == f.User1Id).FirstAsync();
                }
                FriendsList.Add(us.Id);
            }
            return FriendsList;
        }

        public async Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(string UserId)
        {
            var CurrentUser = await _context.Users
                .Include(u => u.FriendshipsInitiated)
                .Include(u => u.FriendshipsReceived)
                .FirstAsync(u => u.Id == UserId);
            return CurrentUser.FriendshipsInitiated.Concat(CurrentUser.FriendshipsReceived);
        }

        public async Task<bool> IsFriendRequestAccepted(string User1Id, string User2Id)
        {
            var fr = await _context.FriendRequest.FirstAsync(fr => fr.RequesterId == User1Id && fr.ReceiverId == User2Id);
            return fr.isAccepted;
        }

        public async Task<bool> IsFriendRequestExistsAsync(string SenderId, string ReceiverId)
        {
            var fr = await _context.FriendRequest.Where(fr => fr.RequesterId == SenderId && fr.ReceiverId == ReceiverId).FirstOrDefaultAsync();
            if (fr == null) return false;
            else return true;
        }
    }
}
