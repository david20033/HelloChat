using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IAppRepository _appRepository;

        public ProfileService(IAppRepository appRepository) 
        {
            _appRepository = appRepository;
        }
        public async Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId)
        {
            
            return await _appRepository.GetProfileViewModelById(ProfileUserId, CurrentUserId);
        }
        public async Task SendFriendRequest(string FromId, string ToId)
        {
            await _appRepository.AddFriendRequest(FromId, ToId);
        }
        public async Task DeleteFriend(string FromId, string ToId)
        {
            await _appRepository.DeleteFriend(FromId, ToId);
        }
        public async Task DeleteFriendRequest(string FromId, string ToId)
        {
            await _appRepository.DeleteFriendRequest(FromId, ToId);
        }
        public async Task AcceptFriendRequest(string FromId, string ToId)
        {
            await _appRepository.AcceptFriendRequest(FromId, ToId);
        }
        public async Task<EditProfileViewModel> GetEditProfileViewModel(string UserId)
        {
            return await _appRepository.GetEditProfileViewModel(UserId);
        }
        public async Task<(string,string)> TryToEditProfile(EditProfileViewModel model)
        {
            if (model.ProfileImage == null)
            {
                await _appRepository.EditProfile(model);
                return ("Success", "Profile is edited successfully");
            }
            string MessageType = "";
            string MessageContent = "";
            var profileImage = model.ProfileImage;
            if (profileImage != null && profileImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(profileImage.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    MessageType = "Error";
                    MessageContent = "Invalid file type. Only JPG, PNG, or GIF files are allowed.";
                    return (MessageType, MessageContent);
                }

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedImages", "ProfilePictures");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                var relativePath = "/UploadedImages/ProfilePictures/" + fileName;
                await _appRepository.UpdateUserPicturePath(model.Id, relativePath);
                await _appRepository.EditProfile(model);
                MessageType = "Success";
                MessageContent = "Profile image uploaded successfully.";
            }
            else
            {
                MessageType = "Error";
                MessageContent = "Please select an image file.";
            }
            return (MessageType, MessageContent);
        }
    }
}
