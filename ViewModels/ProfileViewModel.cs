using HelloChat.Enums;

namespace HelloChat.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }
        public string ProfilePicturePath { get; set; }
        public string FullName { get;set; }
        public string Email { get; set; }
        public FriendshipStatus FriendshipStatus { get; set; }
    }
}
