using System.Security.Claims;
using HelloChat.Controllers;
using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.Repositories.IRepositories;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HelloChat.Repositories
{
    public class AppRepository : IAppRepository
    {
        private readonly HelloChatDbContext _context;
        public AppRepository(HelloChatDbContext context) 
        {
            _context = context;
        }
        public async Task<HomeViewModel> GetHomeViewModelAsync(string CurrentUserId, string User2Id)
        {
            var CurrentUser = await _context.Users
                .Include(u=>u.FriendshipsInitiated)
                .Include(u => u.FriendshipsReceived)
                .FirstAsync(u => u.Id == CurrentUserId);
            var User2 = await _context.Users.FirstOrDefaultAsync(u => u.Id == User2Id);
            var Friendships = CurrentUser.FriendshipsInitiated.Concat(CurrentUser.FriendshipsReceived);
            List<FriendsViewModel> FriendsList = [];
            foreach (var f in Friendships)
            {
                ApplicationUser user = null;
                if (f.User1Id == CurrentUserId)
                {
                    user = await _context.Users.Where(u => u.Id == f.User2Id).FirstAsync();
                }
                else
                {
                    user = await _context.Users.Where(u => u.Id == f.User1Id).FirstAsync();
                }
                var Message = _context.Messages
                    .Where(m=>m.To_id==CurrentUserId&&m.From_id==user.Id)
                    .FirstOrDefault();

                var FriendModel = new FriendsViewModel
                {
                    lastMessage = Message?.Content,
                    Name = $"{user.FirstName} {user.LastName}",
                    ProfileImageUrl = "/images/blank-profile-picture.webp",
                    sentTime = Message?.CreatedDate,
                    UserId = user.Id
                };
                FriendsList.Add(FriendModel);
            }
            var Conversation = await _context.Conversation.Where(c=>(c.User1Id==CurrentUserId&&c.User2Id==User2Id)||
            (c.User2Id == CurrentUserId && c.User1Id == User2Id)).FirstOrDefaultAsync();
            if (Conversation == null&&!User2Id.IsNullOrEmpty())
            {
                Conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    User1Id = CurrentUserId,
                    User2Id = User2Id,
                };
            }
            return new HomeViewModel
            {
                CurrentConversationId = Conversation?.Id ?? Guid.Empty,
                ProfilePicturePath = "/images/blank-profile-picture.webp",
                Friends = FriendsList,
                Messages = Conversation?.Messages ?? new List<Message>(),
                Name = $"{User2?.FirstName} {User2?.LastName}",
            };
        }
        public async Task<List<ApplicationUser>> GetUsersBySearchQuery(string query)
        {
            return await _context
                .Users
                .Where(u=>u.UserName.Contains(query))
                .ToListAsync();
        }
        public async Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId )
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == ProfileUserId);
            var FriendShipStatus = FriendshipStatus.NoFriends;

            if(!_context.FriendRequest.Where(fr=>fr.RequesterId==CurrentUserId&& fr.ReceiverId == ProfileUserId).IsNullOrEmpty())
            {
                if(_context.FriendRequest.Where(fr => fr.RequesterId == CurrentUserId && fr.ReceiverId == ProfileUserId).First().isAccepted)
                {
                    FriendShipStatus = FriendshipStatus.Friends;
                }
                else
                {
                    FriendShipStatus = FriendshipStatus.FriendRequestSend;
                }
            }
            else if (!_context.FriendRequest.Where(fr => fr.RequesterId == ProfileUserId && fr.ReceiverId == CurrentUserId).IsNullOrEmpty())
            {
                if(_context.FriendRequest.Where(fr => fr.RequesterId == ProfileUserId && fr.ReceiverId == CurrentUserId).First().isAccepted)
                {
                    FriendShipStatus = FriendshipStatus.Friends;
                }
                else
                {
                    FriendShipStatus = FriendshipStatus.FriendRequestReceived;
                }
            }
            return new ProfileViewModel
            {
                Email = user?.Email,
                FullName = user?.FirstName + " " + user?.LastName,
                Id = ProfileUserId,
                ProfilePicturePath = user?.ProfilePicturePath,
                FriendshipStatus = FriendShipStatus
            };
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

        public async Task DeleteFriend(string FromId, string ToId)
        {
            if (FromId == ToId || FromId.IsNullOrEmpty() || ToId.IsNullOrEmpty()) return;
            var FriendShip = await _context.Friendship
                .FirstOrDefaultAsync(fr => (fr.User1Id == FromId && fr.User2Id == ToId) 
                || (fr.User2Id == FromId && fr.User1Id == ToId));
            var FriendRequest = await _context.FriendRequest
                .FirstOrDefaultAsync(fr => (fr.ReceiverId == FromId && fr.RequesterId == ToId)
                || (fr.RequesterId == FromId && fr.ReceiverId == ToId));

            if (FriendShip != null&&FriendRequest!=null)
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

        public async Task AcceptFriendRequest(string FromId, string ToId)
        {
            if (FromId == ToId || FromId.IsNullOrEmpty() || ToId.IsNullOrEmpty()) return;
            var Request = await _context.FriendRequest
                .FirstOrDefaultAsync(fr => fr.RequesterId == ToId && fr.ReceiverId == FromId);
            if (Request == null)
            {
                return;
            }
            Request.isAccepted=true;
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
    }
}
