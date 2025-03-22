using System.Drawing.Printing;
using System.Security.Claims;
using HelloChat.Controllers;
using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.Repositories.IRepositories;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public async Task<List<FriendsViewModel>> GetFriendsViewModelAsync(string CurrentUserId)
        {
            var CurrentUser = await _context.Users
                .Include(u => u.FriendshipsInitiated)
                .Include(u => u.FriendshipsReceived)
                .FirstAsync(u => u.Id == CurrentUserId);
            //var User2 = await _context.Users.FirstOrDefaultAsync(u => u.Id == User2Id);
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
                    .Where(m => m.To_id == CurrentUserId && m.From_id == user.Id)
                    .OrderByDescending(m => m.CreatedDate)
                    .FirstOrDefault();
                var FriendModel = new FriendsViewModel
                {
                    lastMessage = Message?.Content,
                    Name = $"{user.FirstName} {user.LastName}",
                    ProfileImageUrl = user.ProfilePicturePath ?? "",
                    sentTime = Message?.CreatedDate,
                    UserId = user.Id,
                    ConversationId = Message?.ConversationId,
                    isLastMessageSeen=Message?.isSeen
                };
                if (await GetConversationAsync(CurrentUserId, user.Id) == null)
                {
                    var el = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        User1Id = CurrentUserId,
                        User2Id = user.Id,
                    };
                    FriendModel.ConversationId = el.Id;
                    await _context.Conversation.AddAsync(el);
                    await _context.SaveChangesAsync();
                }
                else if (FriendModel.ConversationId == null)
                {
                    var el = await GetConversationAsync(CurrentUserId, user.Id);
                    FriendModel.ConversationId = el?.Id;
                }
                FriendsList.Add(FriendModel);
            }
            return FriendsList;
            //var Conversation = await GetConversationAsync(CurrentUserId, User2Id);
            //if (Conversation == null && !User2Id.IsNullOrEmpty())
            //{
            //    Conversation = new Conversation
            //    {
            //        Id = Guid.NewGuid(),
            //        User1Id = CurrentUserId,
            //        User2Id = User2Id,
            //    };
            //    await _context.Conversation.AddAsync(Conversation);
            //    await _context.SaveChangesAsync();
            //}
            //Message? lastSeenMessage = null;
            //if (!User2Id.IsNullOrEmpty())
            //{
            //    lastSeenMessage = Conversation
            //        .Messages
            //        .Where(m => m.isSeen == true && m.To_id == User2Id)
            //        .OrderByDescending(m => m.SeenTime)
            //        .FirstOrDefault();
            //}
            //string active = await GetUserActiveString(User2Id);
            //return new HomeViewModel
            //{
            //    CurrentConversationId = Conversation?.Id ?? Guid.Empty,
            //    ProfilePicturePath = User2?.ProfilePicturePath??"",
            //    Friends = FriendsList,
            //    Messages = Conversation?.Messages
            //    .OrderBy(m => m.CreatedDate)
            //    .Skip((pageNumber - 1) * pageSize)  
            //    .Take(pageSize)  
            //    .ToList() ?? new List<Message>(),
            //    ActiveString = active,
            //    Name = $"{User2?.FirstName} {User2?.LastName}",
            //    SenderId = User2Id,
            //    ReceiverId = CurrentUserId,
            //    LastSeenMessageId = lastSeenMessage?.Id,
            //};
        }
        public async Task<HomeViewModel> GetConversationViewModel(Guid ConversationId,string SenderId)
        {
            var ReceiverId= await GetAnotherUserIdInConversationAsync(SenderId, ConversationId);
            var Receiver = await _context.Users.FirstAsync(u=>u.Id==ReceiverId);
            var Conversation = await _context
                .Conversation
                .Include(c=>c.Messages)
                .FirstAsync(c => c.Id == ConversationId);
            var lastSeenMessage = Conversation
                .Messages
                .Where(m => m.isSeen == true && m.To_id == ReceiverId)
                .OrderByDescending(m => m.SeenTime)
                .FirstOrDefault();
            string active = await GetUserActiveString(ReceiverId);
            return new HomeViewModel
            {
                CurrentConversationId = ConversationId,
                Messages = Conversation.Messages
                .OrderByDescending(m => m.CreatedDate)
                .Take(10)
                .OrderBy(m => m.CreatedDate)
                .ToList(),
                SenderId = SenderId,
                ReceiverId = ReceiverId,
                Name = $"{Receiver.FirstName} {Receiver.LastName}",
                ProfilePicturePath = Receiver.ProfilePicturePath,
                LastSeenMessageId= lastSeenMessage?.Id,
                ActiveString= active,
            };
        }
        public async Task<Guid?> GetLastSeenMessageId(Guid ConversationId, string ReceiverId)
        {
            var Conversation = await _context
                .Conversation
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == ConversationId);
            var lastSeenMessage = Conversation
                .Messages
                .Where(m => m.isSeen == true && m.To_id == ReceiverId)
                .OrderByDescending(m => m.SeenTime)
                .FirstOrDefault();
            return lastSeenMessage?.Id;
        }
        public async Task<List<Message>> LoadMessages(Guid ConversationId, int page)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == ConversationId)
                .OrderByDescending(m => m.CreatedDate)
                .Skip((page - 1) * 10)
                .Take(10)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();
        }
        public async Task<List<ApplicationUser>> GetUsersBySearchQuery(string query)
        {
            return await _context
                .Users
                .Where(u => u.UserName.Contains(query))
                .ToListAsync();
        }
        public async Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == ProfileUserId);
            var FriendShipStatus = FriendshipStatus.NoFriends;

            if (!_context.FriendRequest.Where(fr => fr.RequesterId == CurrentUserId && fr.ReceiverId == ProfileUserId).IsNullOrEmpty())
            {
                if (_context.FriendRequest.Where(fr => fr.RequesterId == CurrentUserId && fr.ReceiverId == ProfileUserId).First().isAccepted)
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
                if (_context.FriendRequest.Where(fr => fr.RequesterId == ProfileUserId && fr.ReceiverId == CurrentUserId).First().isAccepted)
                {
                    FriendShipStatus = FriendshipStatus.Friends;
                }
                else
                {
                    FriendShipStatus = FriendshipStatus.FriendRequestReceived;
                }
            }
            if (ProfileUserId == CurrentUserId)
            {
                FriendShipStatus = FriendshipStatus.SameUser;
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
        public async Task<Guid> SendMessageAndReturnItsId(string FromId, string ToId, string Content)
        {
            var Conversation = await GetConversationAsync(FromId, ToId);
            var Message = new Message
            {
                Id = Guid.NewGuid(),
                Conversation = Conversation,
                ConversationId = Conversation.Id,
                From_id = FromId,
                To_id = ToId,
                Content = Content,
                CreatedDate = DateTime.Now,
                isSeen = false,
                SeenTime = null,
            };
            await _context.Messages.AddAsync(Message);
            await _context.SaveChangesAsync();
            return Message.Id;
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
                ApplicationUser us = null;
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
        private async Task<Conversation?> GetConversationAsync(string User1Id, string User2Id)
        {
            return await _context.Conversation
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => (c.User1Id == User1Id && c.User2Id == User2Id)
                || (c.User2Id == User1Id && c.User1Id == User2Id));
        }
        public async Task<Guid> SetSeenToLastMessageAndReturnItsId(string UserId, Guid ConversationId)
        {
            var Conversation = await _context
                .Conversation
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == ConversationId);
            var lastMessage = Conversation
                .Messages
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            if (lastMessage?.From_id == UserId || lastMessage == null) return Guid.Empty;
            lastMessage.isSeen = true;
            lastMessage.SeenTime = DateTime.Now;
            await _context.SaveChangesAsync();
            return lastMessage.Id;
        }
        public async Task<bool> isLastMessageSeen(string UserId, Guid ConversationId)
        {
            var Conversation = await _context
                .Conversation
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == ConversationId);
            var lastMessage = Conversation
                .Messages
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            if (lastMessage == null) return false;
            return lastMessage.isSeen;
        }
        public async Task<string> GetAnotherUserIdInConversationAsync(string UserId, Guid ConversationId)
        {
            var Conversation = await _context
                .Conversation
                .FirstAsync(c => c.Id == ConversationId);
            if (Conversation.User1Id == UserId)
            {
                return Conversation.User2Id;
            }
            else
            {
                return Conversation.User1Id;
            }
        }
        public async Task DeleteMessageContent(Guid MessageId)
        {
            var message = await _context.Messages.Where(m => m.Id == MessageId).FirstOrDefaultAsync();
            if (message == null) return;
            message.Content = "Message Removed";
            message.isDeleted = true;
            await _context.SaveChangesAsync();
        }
        public async Task SetLocalDeleted(Guid MessageId)
        {
            var message = await _context.Messages.Where(m => m.Id == MessageId).FirstOrDefaultAsync();
            if (message == null) return;
            message.isLocalDeleted = true;
            await _context.SaveChangesAsync();
        }
        public async Task SetMessageReaction(Guid MessageId, string From_Id, string To_Id, MessageReaction reaction)
        {
            var Message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == MessageId);
            if (Message == null) return;
            if (Message.From_id == From_Id && Message.To_id == To_Id)
            {
                Message.ReactionFromSender = reaction;
            }
            else if (Message.To_id == From_Id && Message.From_id == To_Id)
            {
                Message.ReactionFromReceiver = reaction;
            }
            await _context.SaveChangesAsync();
        }
        public async Task SetUserActive(string UserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user == null) return;
            user.isActive = true;
            await _context.SaveChangesAsync();
        }
        public async Task SetUserExitActive(string UserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user == null) return;
            user.isActive = false;
            user.LastTimeActive = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        public async Task<string> GetUserActiveString(string UserId){
            string active = "";
            var User = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (User != null && User.isActive)
            {
                active = "Active Now";
            }
            else if (User != null)
            {
                DateTime now = DateTime.Now;
                DateTime lastActive = User.LastTimeActive;
                TimeSpan difference = now - lastActive;
                if (difference < TimeSpan.FromMinutes(5))
                {
                    active = "Last Active: Just Now";
                }
                else if (difference >= TimeSpan.FromMinutes(5) && difference <= TimeSpan.FromMinutes(59))
                {
                    active = $"Last Active: {difference.Minutes.ToString()} Minute/s ago";
                }
                else if (difference >= TimeSpan.FromHours(1) && difference <= TimeSpan.FromHours(23))
                {
                    active = $"Last Active: {difference.Hours.ToString()} Hour/s ago";
                }
                else if (difference >= TimeSpan.FromDays(1))
                {
                    active = $"Last Active: {difference.Days.ToString()} Day/s ago";
                }
            }
            return active;
        }
        public async Task<EditProfileViewModel> GetEditProfileViewModel(string UserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=>u.Id==UserId);
            if (user == null) return null;
            return new EditProfileViewModel
            {
                Id = UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfileImagePath = user.ProfilePicturePath
            };
        }
        public async Task EditProfile(EditProfileViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null) return;
            user.FirstName = model.FirstName;
            user.LastName=model.LastName;
            user.PhoneNumber = model.PhoneNumber.ToString();
            user.Email = model.Email;
            await _context.SaveChangesAsync();
        }
        public async Task UpdateUserPicturePath(string UserId,string PicturePath)
        {
            var user=await _context.Users.FirstOrDefaultAsync(u=>u.Id==UserId);   
            if (user == null) return;
            user.ProfilePicturePath = PicturePath;
            await _context.SaveChangesAsync();
        }

    }
}
