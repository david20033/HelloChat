namespace HelloChat.ViewModels
{
    public class ConversationsViewModel
    {
        public Guid ConversationId { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Name { get; set; }
        public string? lastMessage { get;set; }
        public DateTime? sentTime { get; set; }   

    }
}
