using System.Diagnostics;
using System.Security.Claims;
using HelloChat.Data;
using HelloChat.Models;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelloChat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HelloChatDbContext _context;

        public HomeController(ILogger<HomeController> logger, HelloChatDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        [Authorize]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); 
            }

            var userGuid = Guid.Parse(userId);

            var users = _context.Users.ToList();
            var messages = _context.Messages
                .Where(m => m.To_id == userGuid)
                .ToList();

            var model = users.Select(user =>
            {
                var lastMessage = messages
                    .FirstOrDefault(m => m.From_id == Guid.Parse(user.Id));

                return new ConversationsViewModel
                {
                    ProfileImageUrl = "/images/blank-profile-picture.webp", 
                    lastMessage = lastMessage?.Content,
                    Name = user.UserName,
                    sentTime = lastMessage?.CreatedDate
                };
            }).ToList();
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
