namespace HelloChat.Data
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string HrefId { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
