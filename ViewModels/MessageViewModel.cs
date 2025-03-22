using HelloChat.Data;

namespace HelloChat.ViewModels
{
    public class MessageViewModel
    {
        public List<Message> Messages { get; set; }
        public string LastSeenMessageId { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
    }
}
