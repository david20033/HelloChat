namespace HelloChat.ViewModels
{
    public class FriendsViewModel
    {
        public string ProfileImageUrl { get; set; }
        public string Name { get; set; }
        public string? lastMessage { get; set; }
        public DateTime? sentTime { get; set; }
        public string UserId { get; set; }
        public Guid? ConversationId { get; set; }
        public bool? isLastMessageSeen { get; set; }
    }
}
