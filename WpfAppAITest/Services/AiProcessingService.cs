using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Serilog;
using WpfAppAITest.Helpers;
using WpfAppAITest.Interfaces;
using Xceed.Words.NET;
using Image = System.Drawing.Image;

namespace WpfAppAITest.Services
{
    public class ContentSection
    {
        public string Text { get; set; }
        public string? Image { get; set; }
    }
    public class AiProcessingService
    {
        private readonly HttpClient _httpClient;
        private const string OpenAIApiKey = "your-api-key";
        private readonly IHttpBuilder _http;

        public AiProcessingService(IHttpBuilder http)
        {
            _http = http;
        }

        public async Task<string> TranscribeAudioAsync(string filePath)
        {
            try
            {
                Log.Information("AiProcessingService - TranscribeAudioAsync: Starting transcription to API.");

                using var content = new MultipartFormDataContent();
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav"); // Adjust if needed

                content.Add(streamContent, "audioFile", Path.GetFileName(filePath));

                HttpResponseMessage response = await _http.HttpClient.PostAsync("https://localhost:7190/api/transcribe", content);
                response.EnsureSuccessStatusCode();
                var res = await response.Content.ReadAsStringAsync();

                Log.Information("AiProcessingService - TranscribeAudioAsync: Transcription to API completed.");
                return res;
            }
            catch (Exception e)
            {
                MessageBox.Show("Converting voice to text failed.");
                Log.Error(
                    $"AiProcessingService - TranscribeAudioAsync: Transcription to API failed. Reason: {e.Message}.");
                return string.Empty;
            }

        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return Array.Find(ImageCodecInfo.GetImageDecoders(), codec => codec.FormatID == format.Guid);
        }
        public string CompressImage(string base64String, int quality = 100)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    using (Image image = Image.FromStream(ms))
                    {
                        using (MemoryStream outputStream = new MemoryStream())
                        {
                            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Png);
                            EncoderParameters encoderParameters = new EncoderParameters(1);
                            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                            image.Save(outputStream, jpgEncoder, encoderParameters);
                            return Convert.ToBase64String(outputStream.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error compressing image: " + ex.Message);
                return null;
            }
        }

        public async Task<string> GenerateDocxAsync(List<DocxSection> contentSections, string title)
        {
            var document = new
            {
                title,
                content = contentSections.ConvertAll(section => new ContentSection
                {
                    Text = section.Text,
                    Image = string.IsNullOrEmpty(section.Image) ? null : $"data:image/png;base64,{section.Image}"
                })
            };

            var jsonRequest = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
            using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _http.HttpClient.PostAsync("https://localhost:7190/api/generate-docx", content);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
