using System.ClientModel;
using System.IO;
using System.Threading.Tasks;
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

        public OpenAiService(OpenAIClient client,EmbeddingClient embeddingClient)
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
    }
}
