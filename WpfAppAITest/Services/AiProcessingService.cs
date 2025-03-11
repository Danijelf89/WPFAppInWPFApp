using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using Xceed.Words.NET;
using Image = System.Drawing.Image;

namespace WpfAppAITest.Services
{
    class AiProcessingService
    {
        private readonly HttpClient _httpClient;
        private const string OpenAIApiKey = "your-api-key";

        public AiProcessingService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> TranscribeAudioAsync(string filePath)
        {
            byte[] audioBytes = File.ReadAllBytes(filePath);
            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(audioBytes), "file", "audio.wav" }
            };

            HttpResponseMessage response = await _httpClient.PostAsync("http://localhost:5000/transcribe/", content);
            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
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
                            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

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

        public string GenerateDocxAsync(List<(string text, string base64Image)> contentSections, string title)
        {
            string docxPath = "generated_document.docx";
            using (var doc = DocX.Create(docxPath))
            {
                if (!string.IsNullOrEmpty(title))
                {
                    doc.InsertParagraph(title).FontSize(18).Bold().Alignment = Xceed.Document.NET.Alignment.center;
                }

                foreach (var section in contentSections)
                {
                    doc.InsertParagraph(section.text).SpacingAfter(10);
                    if (!string.IsNullOrEmpty(section.base64Image))
                    {
                        byte[] imageData = Convert.FromBase64String(section.base64Image);
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            var image = doc.AddImage(ms);
                            var picture = image.CreatePicture(300, 300);
                            doc.InsertParagraph().AppendPicture(picture);
                        }
                    }
                }
                doc.Save();
            }
            return docxPath;
        }
    }
}
