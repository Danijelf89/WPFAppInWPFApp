using AI_Manual_dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;

namespace AI_Manual_dotnet_backend.Controllers;


[ApiController]
[Route("api/transcribe")]
public class TranscriptionController : ControllerBase
{
    private readonly TranscriptionService _transcriptionService;

    public TranscriptionController(TranscriptionService transcriptionService)
    {
        _transcriptionService = transcriptionService;
    }

    [HttpPost()]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> TranscribeAudio(IFormFile audioFile)
    {
        if (audioFile == null || audioFile.Length == 0)
            return BadRequest("No audio file provided.");
        if (!audioFile.FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only .wav files are supported.");
        }

        using var convertedStream = ConvertTo16kHz(audioFile);

        var result = await _transcriptionService.TranscribeAudioAsync(convertedStream);

        if (result.success == true)
            return Ok(result.transcription);
        else
            return StatusCode(500, result.transcription);
    }

    private MemoryStream ConvertTo16kHz(IFormFile audioFile)
    {
        using var reader = new WaveFileReader(audioFile.OpenReadStream());
        var format = new WaveFormat(16000, reader.WaveFormat.Channels);  // Convert to 16kHz
        using var resampler = new MediaFoundationResampler(reader, format); // Resample audio
        var memoryStream = new MemoryStream();

        WaveFileWriter.WriteWavFileToStream(memoryStream, resampler); // Write to memory stream
        memoryStream.Position = 0;

        return memoryStream;
    }
}
