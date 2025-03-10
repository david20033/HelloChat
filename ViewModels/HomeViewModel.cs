using HelloChat.Data;

namespace HelloChat.ViewModels
{
    public class HomeViewModel
    {
        public Guid CurrentConversationId { get; set; }
        public string ProfilePicturePath { get; set; }
        public string Name { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<FriendsViewModel> Friends { get; set; }

    }
}
