using System.ComponentModel.DataAnnotations;

namespace HelloChat.Data
{
    public class Friendship
    {
        [Key]
        public Guid Id { get; set; }
        public ApplicationUser User1 { get; set; }
        public string User1Id { get; set; }
        public ApplicationUser User2 { get; set; }
        public string User2Id { get; set; }
        public DateTime FriendShipDate { get; set; }

    }
}
