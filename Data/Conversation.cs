using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HelloChat.Data
{
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("User1Id")]
        public IdentityUser User1 { get; set; }
        public string User1Id { get;set; }
        [ForeignKey("User2Id")]
        public IdentityUser User2 { get; set; }
        public string User2Id { get; set; }
        public ICollection<Message> Messages { get; set; } = [];
    }
} 
