using HelloChat.Data;

namespace HelloChat.Repositories.IRepositories
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotifications(string userId);
        Task RemoveNotificationByHref(string href);
    }
}
