using System.ComponentModel.DataAnnotations;

namespace HelloChat.Data
{
    public class FriendRequest
    {
        [Key]
        public Guid Id { get; set; }
        public ApplicationUser Requester { get; set; }
        public string RequesterId { get; set; }

        public ApplicationUser Receiver { get; set; }
        public string ReceiverId { get; set; }
        public DateTime RequestDate { get; set; }
        public bool isAccepted { get; set; }
    }
}
