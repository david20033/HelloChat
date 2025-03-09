using Microsoft.AspNetCore.Identity;

namespace HelloChat.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? ProfilePicturePath {  get; set; }
        public ICollection<FriendRequest> SentRequests { get; set; }
        public ICollection<FriendRequest> ReceivedRequests { get; set; }
        public ICollection<Friendship> FriendshipsInitiated { get; set; }
        public ICollection<Friendship> FriendshipsReceived { get; set; }
    }
}
