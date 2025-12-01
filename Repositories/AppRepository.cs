using System.Drawing.Printing;
using System.Security.Claims;
using System.Threading.Tasks.Dataflow;
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
        public async Task<ApplicationUser> GetUserByIdAsync(string UserId)
        {
            return await _context.Users
                .Where(u => u.Id == UserId)
                .FirstAsync();
        }
        public async Task<IEnumerable<Friendship>> GetUserFriendshipsAsync(string UserId)
        {
            var CurrentUser = await _context.Users
                .Include(u => u.FriendshipsInitiated)
                .Include(u => u.FriendshipsReceived)
                .FirstAsync(u => u.Id == UserId);
            return CurrentUser.FriendshipsInitiated.Concat(CurrentUser.FriendshipsReceived);
        }
        public async Task<List<Message>> GetAllReceivedMessagesAsync(string Sender,string Receiver)
        {
            return await _context.Messages.Where(m => m.To_id == Sender && m.From_id == Receiver).ToListAsync();
        }
        public async Task<Message?> GetLastMessageAsync(string Sender, string Receiver)
        {
            return await _context.Messages
                .Where(m => (m.From_id == Sender && m.To_id == Receiver) || (m.From_id == Receiver && m.To_id == Sender))
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefaultAsync();
        }
        public async Task AddConversationAsync(Conversation conversation)
        {
            await _context.Conversation.AddAsync(conversation);
            await _context.SaveChangesAsync();
        }
        public async Task<Conversation?> GetConversationByIdAsync(Guid Id)
        {
            return await _context.Conversation
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == Id);
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
        public async Task<List<Message>> LoadImages(Guid ConversationId, int page)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == ConversationId&&m.ImageUrl!=null)
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
        public async Task<bool> IsFriendRequestExistsAsync(string SenderId, string ReceiverId)
        {
            var fr= await _context.FriendRequest.Where(fr => fr.RequesterId == SenderId && fr.ReceiverId == ReceiverId).FirstOrDefaultAsync();
            if(fr==null) return false;
            else return true;
        }
        public async Task<bool> IsFriendRequestAccepted(string User1Id, string User2Id)
        {
            var fr= await _context.FriendRequest.FirstAsync(fr => fr.RequesterId == User1Id && fr.ReceiverId == User2Id);
            return fr.isAccepted;
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
        public async Task AddMessageAsync(Message message)
        {
           await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
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
        public async Task<Conversation?> GetConversationAsync(string User1Id, string User2Id)
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
        public async Task SetMessageSeen(Guid MessageId)
        {
            var message =await _context.Messages.FirstOrDefaultAsync(m=>m.Id== MessageId);
            if (message == null) return;
            message.isSeen = true;
            message.SeenTime = DateTime.Now;
            await _context.SaveChangesAsync();
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
        public async Task<Message> GetMessageByIdAsync(Guid Id)
        {
            return await _context.Messages.FirstAsync(m => m.Id == Id);
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
        public async Task AddNotificationAsync (Notification notification)
         {
            if (_context.Notification.Where(n => n.HrefId == notification.HrefId && n.ApplicationUserId == notification.ApplicationUserId).Any())
            {
                return;
            }
            await _context.Notification.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Notification>> GetUserNotifications(string UserId)
        {
            return await _context.Notification.Where(n=>n.ApplicationUserId == UserId).ToListAsync();
        }
        public async Task RemoveNotificationByHref(string href)
        {
            await _context.Notification.Where(n => n.HrefId == href).ExecuteDeleteAsync();
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

        public async Task<bool> AreFriendsAsync(Guid userId, Guid otherId)
        {
            return await _context.Friendship
                .Where(f => (f.User1Id == userId.ToString() && f.User2Id == otherId.ToString()) ||
                            (f.User2Id == userId.ToString() && f.User1Id == otherId.ToString()))
                .AnyAsync();
        }
        public async Task<string> GetCommonInterestsAsync(string User1Id, string User2Id)
        {
            if (string.IsNullOrWhiteSpace(User1Id) || string.IsNullOrWhiteSpace(User2Id))
                return string.Empty;

            var interestsUser1 = (await _context.Users
                .Where(u => u.Id == User1Id)
                .Select(u => u.Interests)
                .FirstOrDefaultAsync()) ?? string.Empty;
            var interestsUser2 = (await _context.Users
                .Where(u => u.Id == User2Id)
                .Select(u => u.Interests)
                .FirstOrDefaultAsync()) ?? string.Empty;

            var set1 = new HashSet<string>(
                interestsUser1.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(i => i.Trim().ToLower())
            );

            var set2 = new HashSet<string>(
                interestsUser2.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(i => i.Trim().ToLower())
            );

            set1.IntersectWith(set2);

            return string.Join(", ", set1);
        }
    }
}
