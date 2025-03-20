
using System.IO;
using System.Text.Json;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WpfAppAITest.Helpers;
using WpfAppAITest.Interfaces;
using WpfAppAITest.Services;
using WpfAppAITest.ViewModels;
using GemBox.Document;
using SystemPath = System.IO.Path;
using System.Windows.Xps.Packaging;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;


namespace WpfAppAITest.Managers
{
    public class DocumentationManager
    {
        private readonly IServiceProvider _serviceProvider;
        private string _tempXpsPath;
        private FileSystemWatcher _watcher;
        private MainWindowViewModel _mainViewModel;
        private readonly string _filePath = SystemPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "document.docx");

        public DocumentationManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async void CreateDocument(TextPointer contentStart, TextPointer contentEnd, Visibility screensHotLabelVisibility, Canvas canvas)
        {
            try
            {
                var aiProcessingService = _serviceProvider.GetRequiredService<AiProcessingService>();
                using IBusyWindow _busyService = new BusyWindowService();
                await _busyService.ShowAsync("Generating document... Please wait.");
                List<DocxSection> sectionList = [];
                string richText;
                var textRange = new TextRange(contentStart, contentEnd);
                using (var stream = new MemoryStream())
                {
                    textRange.Save(stream, System.Windows.DataFormats.Rtf);
                    richText = Encoding.UTF8.GetString(stream.ToArray());
                }

                string? imageBase64 = null;
                if (screensHotLabelVisibility == Visibility.Collapsed)
                    imageBase64 = DocumentHelper.GetCanvasImageBase64(canvas);

                var section = new DocxSection
                {
                    Text = richText,
                    Image = imageBase64
                };
                sectionList.Add(section);
                var doc = await aiProcessingService.GenerateDocxAsync(sectionList, "Documentation");
                var docString = JsonSerializer.Deserialize<DocxResponse>(doc);
                var base64Docx = docString?.docx;
                DocumentHelper.SaveDocx(base64Docx);
            }
            catch (Exception e)
            {
                Log.Error($"MainViewModel - CreateDocument: Generating document failed. Reason: {e.Message}");
                System.Windows.MessageBox.Show("Generating document failed!");
            }
        }

        public IDocumentPaginatorSource? LoadDocument(MainWindowViewModel mainViewModel,
                    TextPointer contentStart, TextPointer contentEnd, Visibility screensHotLabelVisibility, Canvas canvas)
        {
            try
            {
                _mainViewModel = mainViewModel;
                _tempXpsPath = SystemPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempDocument.docx");
                if (File.Exists(_filePath))
                {
                    using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var document = DocumentModel.Load(stream);

                    document.Save(_tempXpsPath, SaveOptions.XpsDefault);
                }
                else
                {
                    CreateDocument(contentStart, contentEnd, screensHotLabelVisibility, canvas);
                }


                using XpsDocument xpsDoc = new XpsDocument(_tempXpsPath, FileAccess.Read);
                StartWatchingFile();
                return xpsDoc.GetFixedDocumentSequence();
            }
            catch (Exception ex)
            {

                System.Windows.MessageBox.Show("Document loading failed!");
                Log.Error($"MainWindowViewModel - LoadDocument: Error loading document: {ex.Message}");
                return null;
            }
        }

        private void StartWatchingFile()
        {
            using (new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                _watcher = new FileSystemWatcher
                {
                    Path = SystemPath.GetDirectoryName(_filePath) ?? string.Empty,
                    Filter = SystemPath.GetFileName(_filePath),
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _watcher.Changed += OnDocumentChanged;
                _watcher.EnableRaisingEvents = true;
            }
        }

        public void OnDocumentChanged(object sender, FileSystemEventArgs e)
        {
            _mainViewModel.OnDocumentChanged();
        }

        public async void ResetDocument()
        {
            using IBusyWindow busyService = new BusyWindowService();
            await busyService.ShowAsync("Resetting document... Please wait.");
            using var wordDocument = WordprocessingDocument.Create(SystemPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "document.docx"), WordprocessingDocumentType.Document);
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());
            mainPart.Document.Save();
        }
    }
}
