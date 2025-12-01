namespace HelloChat.Services.IServices
{
    public interface IOpenAiService
    {
        Task<string> GenerateTextFromSpeech(byte[] audio);
        Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text);
        string SerializeEmbedding(ReadOnlyMemory<float> vector);
        ReadOnlyMemory<float> DeserializeEmbedding(string json);
         Task<string> GetAiReasonAsync(int mutualCount, string mutualInterest);
    }
}
