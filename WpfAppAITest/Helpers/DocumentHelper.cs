using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;

namespace WpfAppAITest.Helpers
{
    public static class DocumentHelper
    {
        public static void SaveDocx(string base64String)
        { 
            byte[] docxBytes = Convert.FromBase64String(base64String);

            string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(projectRoot, "document.docx");

            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                System.IO.File.WriteAllBytes(filePath, docxBytes);
                Console.WriteLine($"DOCX file saved successfully at {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving the file: {ex.Message}");
            }
        }

        public static string GetCanvasImageBase64(Canvas canvas)
        {
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            if (width == 0 || height == 0)
                return null;

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);

            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(stream);

                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }

    public class DocxSection
    {
        public string? Text { get; set; } = string.Empty;
        public string? Image { get; set; } = null;
    }
}
