using HelloChat.Data;
using HelloChat.Repositories.IRepositories;
using HelloChat.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HelloChat.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HelloChatDbContext _context;
        public UserRepository(HelloChatDbContext context)
        {
            _context = context;
        }

        public async Task EditProfile(EditProfileViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null) return;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber.ToString();
            user.Email = model.Email;
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetUserActiveString(string UserId)
        {
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

        public async Task<ApplicationUser> GetUserByIdAsync(string UserId)
        {
            return await _context.Users
                .Where(u => u.Id == UserId)
                .FirstAsync();
        }

        public async Task<List<ApplicationUser>> GetUsersBySearchQuery(string query)
        {
            return await _context
                .Users
                .Where(u => u.UserName.Contains(query))
                .ToListAsync();
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

        public async Task UpdateUserPicturePath(string UserId, string PicturePath)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user == null) return;
            user.ProfilePicturePath = PicturePath;
            await _context.SaveChangesAsync();
        }
    }
}
