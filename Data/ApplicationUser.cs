using Microsoft.AspNetCore.Identity;

namespace HelloChat.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? ProfilePicturePath { get; set; } = "/images/blank-profile-picture.webp";
        public bool isActive { get; set; } = true;
        public DateTime LastTimeActive { get; set; } = DateTime.Now;
        public ICollection<FriendRequest> SentRequests { get; set; }
        public ICollection<FriendRequest> ReceivedRequests { get; set; }
        public ICollection<Friendship> FriendshipsInitiated { get; set; }
        public ICollection<Friendship> FriendshipsReceived { get; set; }
    }
}
