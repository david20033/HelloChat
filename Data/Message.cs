using HelloChat.Enums;

namespace HelloChat.Data
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid From_id { get; set; }
        public Guid To_id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool isSeen { get; set; }
        public DateTime? SeenTime { get; set; }
        public MessageReaction Reaction { get; set; } = MessageReaction.None;

    }
}
