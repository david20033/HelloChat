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
        private readonly IAppRepository _appRepository;

        public ProfileService(IAppRepository appRepository) 
        {
            _appRepository = appRepository;
        }
        public async Task<ProfileViewModel> GetProfileViewModelById(string ProfileUserId, string CurrentUserId)
        {

            var user = await _appRepository.GetUserByIdAsync(ProfileUserId);
            var FriendShipStatus = FriendshipStatus.NoFriends;

            if (await _appRepository.IsFriendRequestExistsAsync(CurrentUserId, ProfileUserId))
            {
                if (await _appRepository.IsFriendRequestAccepted(CurrentUserId,ProfileUserId))
                {
                    FriendShipStatus = FriendshipStatus.Friends;
                }
                else
                {
                    FriendShipStatus = FriendshipStatus.FriendRequestSend;
                }
            }
            else if (await _appRepository.IsFriendRequestExistsAsync(ProfileUserId, CurrentUserId))
            {
                if (await _appRepository.IsFriendRequestAccepted(ProfileUserId, CurrentUserId))
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
            var user = await _appRepository.GetUserByIdAsync(UserId);
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
