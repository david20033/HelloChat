using System.Security.Claims;
using AspNetCoreGeneratedDocument;
using HelloChat.Data;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HelloChat.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IFriendRecommendationService _recommendationService;

        public ProfileController(IProfileService profileService, IFriendRecommendationService recommendationService) 
        {
            _profileService = profileService;
            _recommendationService = recommendationService;
        }
        public async Task<IActionResult> Index(string id)
        {
            var ProfileUserId = id;
            var CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await _profileService.GetProfileViewModelById(ProfileUserId, CurrentUserId);
            await _profileService.RemoveNotificationAsync(id);
            ViewBag.FromId = CurrentUserId;
            ViewBag.FromUserName = await _profileService.GetUserNameById(CurrentUserId);
            return View(model);
        }
        public async Task<IActionResult> GetUserNotifications()
        {
            var CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Notifications =await _profileService.GetNotificationsAsync(CurrentUserId);
            return Json(Notifications);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFriendRequest(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id.IsNullOrEmpty()||currentUserId.IsNullOrEmpty())
            {
                return RedirectToAction("Index", new { id = id });
            }
            await _profileService.SendFriendRequest(currentUserId, id);
            return RedirectToAction("Index" ,new {id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFriend(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id.IsNullOrEmpty() || currentUserId.IsNullOrEmpty())
            {
                return RedirectToAction("Index", new { id });
            }
            await _profileService.DeleteFriend(currentUserId, id);
            return RedirectToAction("Index", new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFriendRequest(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id.IsNullOrEmpty() || currentUserId.IsNullOrEmpty())
            {
                return RedirectToAction("Index", new {id });
            }
            await _profileService.DeleteFriendRequest(currentUserId, id);
            return RedirectToAction("Index", new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptFriendRequest(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id.IsNullOrEmpty() || currentUserId.IsNullOrEmpty())
            {
                return RedirectToAction("Index", new { id });
            }
            await _profileService.AcceptFriendRequest(currentUserId, id);
            return RedirectToAction("Index", new { id });
        }
        public async Task<IActionResult> Edit()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId.IsNullOrEmpty())
            {
                return RedirectToAction("Index");
            }
            var model = await _profileService.GetEditProfileViewModel(currentUserId);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid) 
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId.IsNullOrEmpty())
                {
                    return RedirectToAction("Edit");
                }
                var result = await _profileService.TryToEditProfile(model);
                if (result.Item1 == "Error")
                {
                    TempData["Error"]=result.Item2;
                    return RedirectToAction("Edit");
                }
                return RedirectToAction("Index", new { id = model.Id });
            }
            else
            {
                return RedirectToAction("Edit");
            }
        }
        public async Task<IActionResult> RecommendFriends()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId.IsNullOrEmpty())
            {
                return RedirectToAction("Index", "Home");
            }
            var model = await _recommendationService.RecommendFriendsAsync(Guid.TryParse(currentUserId, out Guid Id) ? Id : Guid.Empty);
            if (model == null) return NotFound();

            return Json(model);
        }

    }
}
