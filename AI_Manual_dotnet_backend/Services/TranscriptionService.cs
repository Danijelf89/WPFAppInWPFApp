using System.Text;
using Whisper.net;

namespace AI_Manual_dotnet_backend.Services
{
    public class TranscriptionService
    {
        private readonly WhisperFactory _whisperFactory;

        public TranscriptionService(WhisperFactory whisperFactory)
        {
            _whisperFactory = whisperFactory;
        }

        public async Task<(string transcription, bool success)> TranscribeAudioAsync(Stream audioStream)
        {
            try
            {
                using var processor = _whisperFactory.CreateBuilder()
                .WithLanguage("auto") // Automatically detects language
                .Build();

                StringBuilder transcription = new();

                await foreach (var result in processor.ProcessAsync(audioStream))
                {
                    transcription.Append(result.Text);
                }

                return (transcription.ToString(), true);
            }
            catch (Exception ex)
            {
                return ("Transcription failed:" + ex.Message, false);
            }
            
        }
    }
}
