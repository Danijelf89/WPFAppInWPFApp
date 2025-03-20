
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
using SystemControls = System.Windows.Controls;
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

        public async Task<bool> CreateDocument(SystemControls.RichTextBox richTextBox, Visibility screensHotLabelVisibility, Canvas canvas)
        {
            try
            {
                var aiProcessingService = _serviceProvider.GetRequiredService<AiProcessingService>();
                using IBusyWindow _busyService = new BusyWindowService();
                await _busyService.ShowAsync("Generating document... Please wait.");
                List<DocxSection> sectionList = [];
                string richText;
                var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
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
                return true;
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(IOException))
                {
                    System.Windows.MessageBox.Show(
                        "Some other program is using this document. Please close it to continue!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    System.Windows.MessageBox.Show("Generating document failed!");
                }

                Log.Error($"DocumentationManager - CreateDocument: Generating document failed. Reason: {e.Message}");
                return false;
            }
        }

        public async Task<IDocumentPaginatorSource?> LoadDocument(MainWindowViewModel mainViewModel,
                    SystemControls.RichTextBox richTextBox, Visibility screensHotLabelVisibility, Canvas canvas)
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
                    await CreateDocument(richTextBox, screensHotLabelVisibility, canvas);
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
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnDocumentChanged;
                _watcher.Dispose();
            }

            _watcher = new FileSystemWatcher
            {
                Path = SystemPath.GetDirectoryName(_filePath) ?? string.Empty,
                Filter = SystemPath.GetFileName(_filePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            _watcher.Changed += OnDocumentChanged;
            _watcher.EnableRaisingEvents = true;
        }

        public void OnDocumentChanged(object sender, FileSystemEventArgs e)
        {
            _mainViewModel.OnDocumentChanged();
        }

        public async void ResetDocument()
        {
            try
            {
                using IBusyWindow busyService = new BusyWindowService();
                await busyService.ShowAsync("Resetting document... Please wait.");
                using var wordDocument = WordprocessingDocument.Create(
                    SystemPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "document.docx"),
                    WordprocessingDocumentType.Document);
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                mainPart.Document.Save();
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(IOException))
                {
                    System.Windows.MessageBox.Show(
                        "Some other program is using this document. Please close it to continue!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    System.Windows.MessageBox.Show("Reseing document failed!");
                }
                Log.Error($"DocumentationManager - ResetDocument: Reseting document failed. Reason: {e.Message}");
            }

        }
    }
}
