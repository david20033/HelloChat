using HelloChat.Data;
using HelloChat.Models;
using HelloChat.Repositories;
using HelloChat.Repositories.IRepositories;
using HelloChat.Services;
using HelloChat.Services.IServices;
using Microsoft.AspNetCore.Identity;
using static System.Formats.Asn1.AsnWriter;

public class FriendRecommendationService : IFriendRecommendationService
{
    private readonly double _embeddingWeight = 0.7;
    private readonly double _mutualWeight = 0.3;
    private readonly int _mutualMax = 10;
    private readonly IUserEmbeddingRepository _embedRepo;
    private readonly IFriendRepository _friendRepo;
    private readonly IOpenAiService _openAi;
    private readonly UserManager<ApplicationUser> _userManager;
    public FriendRecommendationService(
IUserEmbeddingRepository embedRepo,
IFriendRepository friendRepo,
IOpenAiService openAi,
UserManager<ApplicationUser> userManager)
    {
        _embedRepo = embedRepo;
        _friendRepo = friendRepo;
        _openAi = openAi;
        _userManager = userManager;
    }

    public async Task<List<RecommendationResult>> RecommendFriendsAsync(Guid userId, int take = 10)
    {
        var currentEmbedding = await _embedRepo.GetUserByIdAsync(userId);
        if (currentEmbedding == null)
            return new List<RecommendationResult>();


        var currentVec = _openAi.DeserializeEmbedding(currentEmbedding.EmbeddingJson);


        var others = await _embedRepo.GetAllExceptUserAsync(userId);


        var userFriends = await _friendRepo.GetFriendsAsync(userId);
        var excluded = new HashSet<Guid>(userFriends) { userId };


        var results = new List<RecommendationResult>();


        foreach (var other in others)
        {
            if (excluded.Contains(other.Id)) continue; 


            var otherVec = _openAi.DeserializeEmbedding(other.EmbeddingJson);
            var embeddingScore = CosineSimilarity(currentVec, otherVec); 


            var mutualCount = await _friendRepo.CountMutualFriendsAsync(userId, other.Id);
            var mutualScore = CalculateMutualScore(mutualCount);
            var mutualInterests= await _friendRepo.GetCommonInterestsAsync(userId.ToString(), other.Id.ToString());

            var finalScore = (_embeddingWeight * embeddingScore) + (_mutualWeight * mutualScore);

            var Reasoning = await _openAi.GetAiReasonAsync(mutualCount, mutualInterests);
            results.Add(new RecommendationResult
            {
                UserId = other.Id,
                EmbeddingScore = embeddingScore,
                MutualFriendCount = mutualCount,
                MutualScore = mutualScore,
                FinalScore = finalScore,
                MutualInterests = mutualInterests,
                Reasoning = Reasoning
            });
        }


        return results
        .OrderByDescending(r => r.FinalScore)
        .Take(take)
        .ToList();
    }


    private double CalculateMutualScore(int mutualCount)
    {
        var score = Math.Min(mutualCount, _mutualMax) / (double)_mutualMax; 
        return score;
    }
    private double CosineSimilarity(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
    {
        var aSpan = a.Span;
        var bSpan = b.Span;
        if (a.Length == 0 || b.Length == 0) return 0.0;
        if (a.Length != b.Length) return 0.0; 


        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        {
            var av = aSpan[i];
            var bv = bSpan[i];
            dot += av * bv;
            na += av * av;
            nb += bv * bv;
        }


        if (na == 0 || nb == 0) return 0.0;
        return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }
}
