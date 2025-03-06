using System.ComponentModel.DataAnnotations;
using HelloChat.Enums;

namespace HelloChat.Data
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid From_id { get; set; }
        [Required]
        public Guid To_id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool isSeen { get; set; }
        public DateTime? SeenTime { get; set; }
        public MessageReaction Reaction { get; set; } = MessageReaction.None;

    }
}
