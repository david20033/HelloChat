using HelloChat.Services.IServices;
using Microsoft.CodeAnalysis;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Embeddings;

namespace HelloChat.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAIClient _client;
        private readonly EmbeddingClient _embeddingClient;

        public OpenAiService(OpenAIClient client, EmbeddingClient embeddingClient)
        {
            _client = client;
            _embeddingClient = embeddingClient;
        }

        public async Task<string> GenerateTextFromSpeech(byte[] audio)
        {
            var audioClient = _client.GetAudioClient("whisper-1");

            using var stream = new MemoryStream(audio);

            var options = new AudioTranscriptionOptions
            {
                ResponseFormat = AudioTranscriptionFormat.Text,
            };

            var transcriptionResult = await audioClient.TranscribeAudioAsync(stream, "audio.wav", options);

            return transcriptionResult.Value.Text;
        }
        public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
        {
            OpenAIEmbedding Embedding = await _embeddingClient.GenerateEmbeddingAsync(text);
            return Embedding.ToFloats();
        }
        public string SerializeEmbedding(ReadOnlyMemory<float> vector)
        {
            return System.Text.Json.JsonSerializer.Serialize(vector.ToArray());
        }

        public ReadOnlyMemory<float> DeserializeEmbedding(string json)
        {
            float[] arr = System.Text.Json.JsonSerializer.Deserialize<float[]>(json);
            return new ReadOnlyMemory<float>(arr);
        }
        public async Task<string> GetAiReasonAsync(int mutualCount, string mutualInterest)
        {
            var prompt = $@"
Give a short natural-sounding reason why user sourceUser might want to add TargetUser'.
Context:
- Mutual friends: {mutualCount}
- Similar interests: {(string.IsNullOrWhiteSpace(mutualInterest) ? "None" : mutualInterest)}

Instructions:
- Return only the reason, 4–8 words.
- If there are mutual friends, mention the count.
- If there are similar interests, mention the interest and its related field.
- If there are no mutual interests, do NOT make any up.
- If there is no clear reason, return exactly: 'Friend Suggestion'.";

            var chatClient = _client.GetChatClient("gpt-4o-mini");

            var messages = new List<OpenAI.Chat.ChatMessage>
    {
        new OpenAI.Chat.SystemChatMessage( "You are a helpful assistant that provides friend recommendation reasons"),
        new OpenAI.Chat.UserChatMessage( prompt)
    };

            var response = await chatClient.CompleteChatAsync(messages.ToArray());

            var text = response.Value.Content[0].Text;

            return text ?? string.Empty;
        }

    }

}
