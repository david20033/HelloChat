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
        public async Task<IActionResult> Index([FromQuery]string id)
        {
            var model = await _profileService.GetProfileViewModelById(id);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFriendRequest(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id.IsNullOrEmpty()||currentUserId.IsNullOrEmpty())
            {
                return RedirectToAction("Index");
            }
            await _profileService.SendFriendRequest(currentUserId, id);
            return RedirectToAction("Index");
        }
    }
}
