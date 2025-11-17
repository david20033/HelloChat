using HelloChat.Services.IServices;
using Microsoft.CodeAnalysis;
using OpenAI;
using OpenAI.Audio;
using System.IO;
using System.Threading.Tasks;

namespace HelloChat.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAIClient _client;

        public OpenAiService(OpenAIClient client)
        {
            _client = client;
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
    }
}
