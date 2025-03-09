using System.Security.Claims;
using HelloChat.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HelloChat.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService) 
        {
            _profileService = profileService;
        }
        public async Task<IActionResult> Index(string id)
        {
            var ProfileUserId = id;
            var CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await _profileService.GetProfileViewModelById(ProfileUserId, CurrentUserId);
            return View(model);
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
                return RedirectToAction("Index", new { id = id });
            }
            await _profileService.DeleteFriend(currentUserId, id);
            return RedirectToAction("Index", new { id });
        }
    }
}
