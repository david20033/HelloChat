namespace HelloChat.Models
{
    public class RecommendationResult
    {
        public Guid UserId { get; set; }
        public double EmbeddingScore { get; set; }
        public int MutualFriendCount { get; set; }
        public double MutualScore { get; set; }
        public double FinalScore { get; set; }
    }
}
