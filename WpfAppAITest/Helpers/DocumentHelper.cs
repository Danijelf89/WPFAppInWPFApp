using System.IO;

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
    }

    public class DocxSection
    {
        public string? Text { get; set; } = string.Empty;
        public string? Image { get; set; } = null;
    }
}
