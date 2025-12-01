using HelloChat.Data;
using HelloChat.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HelloChat.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly HelloChatDbContext _context;
        public NotificationRepository(HelloChatDbContext context)
        {
            _context = context;
        }

        public async Task AddNotificationAsync(Notification notification)
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
            return await _context.Notification.Where(n => n.ApplicationUserId == UserId).ToListAsync();
        }

        public async Task RemoveNotificationByHref(string href)
        {
            await _context.Notification.Where(n => n.HrefId == href).ExecuteDeleteAsync();
        }
    }
}
