namespace HelloChat.Services.IServices
{
    public interface IOpenAiService
    {
        Task<string> GenerateTextFromSpeech(byte[] audio);
    }
}
