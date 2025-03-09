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
        public async Task<List<ConversationsViewModel>> GetConversationsAsync(string userGuid)
        {
            var users = await _context.Users.ToListAsync();
            var messages = await _context.Messages
                .Where(m => m.To_id == userGuid)
                .ToListAsync();

            var model = users.Select(user =>
            {
                var lastMessage = messages
                    .FirstOrDefault(m => m.From_id == user.Id);

                return new ConversationsViewModel
                {
                    ProfileImageUrl = "/images/blank-profile-picture.webp",
                    lastMessage = lastMessage?.Content,
                    Name = user.UserName,
                    sentTime = lastMessage?.CreatedDate
                };
            }).ToList();

            return model;
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
            if (!_context.Friendship.Where(fr=>(fr.User1Id== ProfileUserId&& fr.User2Id==ProfileUserId)
            ||(fr.User2Id == ProfileUserId && fr.User1Id == ProfileUserId)).IsNullOrEmpty())
            {
                FriendShipStatus = FriendshipStatus.NoFriends;
            }
            else if(!_context.FriendRequest.Where(fr=>fr.RequesterId==CurrentUserId&& fr.ReceiverId == ProfileUserId).IsNullOrEmpty())
            {
                FriendShipStatus = FriendshipStatus.FriendRequestSend;
            }
            else if (!_context.FriendRequest.Where(fr => fr.RequesterId == ProfileUserId && fr.ReceiverId == CurrentUserId).IsNullOrEmpty())
            {
                FriendShipStatus = FriendshipStatus.FriendRequestReceived;
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
    }
}
