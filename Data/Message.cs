using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HelloChat.Enums;
using Microsoft.AspNetCore.Identity;

namespace HelloChat.Data
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("ConversationId")]
        public Conversation Conversation { get; set; }
        public Guid ConversationId { get; set; }

        public string From_id { get; set; }
        [ForeignKey("From_id")]
        public ApplicationUser From_User { get; set; }

        public string To_id { get; set; }
        [ForeignKey("To_id")]
        public ApplicationUser To_User { get; set; }

        [MaxLength(1000)]
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public byte[]? AudioFile { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool isSeen { get; set; }
        public DateTime? SeenTime { get; set; }
        public MessageReaction ReactionFromReceiver { get; set; } = MessageReaction.None;
        public MessageReaction ReactionFromSender { get; set; } = MessageReaction.None;
        public bool isDeleted { get; set; } = false;
        public bool isLocalDeleted { get; set; } = false;

    }
}
