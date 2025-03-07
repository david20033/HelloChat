using HelloChat.Data;
using HelloChat.Repositories.IRepositories;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
    }
}
