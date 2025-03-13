using NAudio.Wave;

namespace WpfAppAITest.Services
{
    public class TranscriptionService
    {
        private WaveInEvent waveSource;
        private WaveFileWriter waveFile;
        private string outputFilePath = "recorded_audio.wav"; // Path to save the file

        public void StartRecording()
        {
            waveSource = new WaveInEvent();
            waveSource.WaveFormat = new WaveFormat(44100, 1); // 44.1 kHz, Mono

            waveFile = new WaveFileWriter(outputFilePath, waveSource.WaveFormat);

            waveSource.DataAvailable += (s, e) =>
            {
                waveFile?.Write(e.Buffer, 0, e.BytesRecorded);
            };

            
            waveSource.StartRecording();
        }

        public void StopRecording()
        {
            waveSource?.StopRecording();
            waveSource?.Dispose();
            waveFile?.Dispose();
        }

        public string GetRecordedFilePath()
        {
            return outputFilePath;
        }
    }
}
