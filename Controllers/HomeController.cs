using System.Diagnostics;
using System.Security.Claims;
using HelloChat.Data;
using HelloChat.Models;
using HelloChat.Services;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HelloChat.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFriendRecommendationService _friendRecommendationService;
        private readonly IFriendService _friendService;
        private readonly IConversationService _convService;

        public HomeController(ILogger<HomeController> logger, 
            IFriendRecommendationService friendRecommendationService,
            IFriendService friendService,
            IConversationService conversationService)
        {
            _logger = logger;
            _friendRecommendationService = friendRecommendationService;
            _friendService = friendService;
            _convService = conversationService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var model = await _friendService.GetFriendsViewModelAsync(userId);

            var recommendedFriendsResult =
                await _friendRecommendationService.RecommendFriendsAsync(Guid.Parse(userId), 5);
            var recommendedFriendsModel = await _friendService
                .MapFromRecommendationResultToRecommendedFriendViewModel(recommendedFriendsResult);
            ViewBag.RecommendedFriends = recommendedFriendsModel;
            //ViewBag.RecommendedFriends = new List<RecommendedFriendViewModel>();
            ViewBag.FromId = userId;

            return View(model);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SendMessage(string ToId, string Content)
        //{
        //    var FromId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(FromId))
        //    {
        //        return Unauthorized();
        //    }
        //    await _homeService.SendMessage(FromId,ToId,Content);
        //    return RedirectToAction("Index", new { id = ToId });
        //}
        public async Task<IActionResult> SearchUsers( string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new { });
            }
            var users = await _friendService.GetIdentityUsersBySearchQuery(query);
            Console.WriteLine(Json(users));
            return Json(users);
        }
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query)) 
            {
                return RedirectToAction("Index");
            }
            var users= await _friendService.GetIdentityUsersBySearchQuery(query);
            return View(users);
        }
        public async Task<IActionResult> LoadConversation(string conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var Conversation  = await _convService.GetConversationViewModel(Guid.Parse(conversationId),userId);
            return PartialView("_MessageContainerPartial", Conversation);
        }
        public async Task<IActionResult> LoadMessages(string conversationId, int page,string SenderId)
        {
            var ReceiverId = await _convService.GetAnotherUserId(Guid.Parse(conversationId), SenderId);
            var LastSeenMessage = await _convService.GetLastSeenMessageId(Guid.Parse(conversationId), ReceiverId);
            ViewData["LastSeenMessageId"] = LastSeenMessage;
            ViewData["SenderId"] = SenderId;
            ViewData["ReceiverId"] = ReceiverId;
            var messages =await _convService.LoadMessages(Guid.Parse(conversationId),page);
            return PartialView("_MessagesPartial", messages);
        }
        public async Task<IActionResult> LoadInfo(string ConversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var Info = await _convService.GetInfoViewModel(Guid.Parse(ConversationId), userId);
            return PartialView("_InfoPartial", Info);
        }
        public async Task<IActionResult> LoadImages(string conversationId, int page)
        {
            var images = await _convService.LoadImages(Guid.Parse(conversationId), page);
            return PartialView("_ImagesPartial", images);
        }
        [HttpPost]
        public  IActionResult UploadAudio(IFormFile file)
        {
            Console.WriteLine(file);
            return View();
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
