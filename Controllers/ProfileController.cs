using HelloChat.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace HelloChat.Controllers
{
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
    }
}
