using HelloChat.Data;
using HelloChat.Enums;
using HelloChat.Repositories.IRepositories;
using HelloChat.Services.IServices;
using HelloChat.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelloChat.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IFriendRepository _friendRepo;
        public ProfileService(
            IUserRepository userRepo,
            INotificationRepository notificationRepository,
            IFriendRepository friendRepo) 
        {
            _userRepo = userRepo;
            _notificationRepo = notificationRepository;
            _friendRepo = friendRepo;
        }
        public async Task RemoveNotificationAsync(string hrefId)
        {
            await _notificationRepo.RemoveNotificationByHref(hrefId);
        }
        public async Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId)
        {

            var user = await _userRepo.GetUserByIdAsync(ProfileUserId);
            var FriendShipStatus = FriendshipStatus.NoFriends;

            if (await _friendRepo.IsFriendRequestExistsAsync(CurrentUserId, ProfileUserId))
            {
                if (await _friendRepo.IsFriendRequestAccepted(CurrentUserId,ProfileUserId))
                {
                    FriendShipStatus = FriendshipStatus.Friends;
                }
                else
                {
                    FriendShipStatus = FriendshipStatus.FriendRequestSend;
                }
            }
            else if (await _friendRepo.IsFriendRequestExistsAsync(ProfileUserId, CurrentUserId))
            {
                if (await _friendRepo.IsFriendRequestAccepted(ProfileUserId, CurrentUserId))
                {
                    FriendShipStatus = FriendshipStatus.Friends;
                }
                else
                {
                    FriendShipStatus = FriendshipStatus.FriendRequestReceived;
                }
            }
            if (ProfileUserId == CurrentUserId)
            {
                FriendShipStatus = FriendshipStatus.SameUser;
            }
            return new ProfileViewModel
            {
                Email = user?.Email,
                FullName = user?.FirstName + " " + user?.LastName,
                Id = ProfileUserId,
                ProfilePicturePath = user?.ProfilePicturePath,
                FriendshipStatus = FriendShipStatus
            };
        }
       
        public async Task<EditProfileViewModel> GetEditProfileViewModel(string UserId)
        {
            var user = await _userRepo.GetUserByIdAsync(UserId);
            if (user == null) return null;
            return new EditProfileViewModel
            {
                Id = UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfileImagePath = user.ProfilePicturePath
            };
        }
        public async Task<(string,string)> TryToEditProfile(EditProfileViewModel model)
        {
            if (model.ProfileImage == null)
            {
                await _userRepo.EditProfile(model);
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
                await _userRepo.UpdateUserPicturePath(model.Id, relativePath);
                await _userRepo.EditProfile(model);
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
        public async Task<string> GetUserNameById(string id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return "";
            return user.FirstName+" "+ user.LastName;
        }
        public async Task<List<Notification>> GetNotificationsAsync(string UserId)
        {
            return await _notificationRepo.GetUserNotifications(UserId);
        }
        
    }
}
