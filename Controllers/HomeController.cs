using System.Diagnostics;
using System.Security.Claims;
using HelloChat.Data;
using HelloChat.Models;
using HelloChat.Services;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelloChat.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;

        public HomeController(ILogger<HomeController> logger, IHomeService homeService)
        {
            _logger = logger;
            _homeService = homeService;
        }
        public async Task<IActionResult> Index(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); 
            }
            var model = await _homeService.GetConversationsViewModel(userId,id);
            
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string ToId, string Content)
        {
            var FromId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(FromId))
            {
                return Unauthorized();
            }
            await _homeService.SendMessage(FromId,ToId,Content);
            return RedirectToAction("Index", new { id = ToId });
        }
        public async Task<IActionResult> SearchUsers( string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new { });
            }
            var users = await _homeService.GetIdentityUsersBySearchQuery(query);
            Console.WriteLine(Json(users));
            return Json(users);
        }
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query)) 
            {
                return RedirectToAction("Index");
            }
            var users= await _homeService.GetIdentityUsersBySearchQuery(query);
            return View(users);
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
