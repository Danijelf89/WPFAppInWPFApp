using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using WpfAppAITest.ViewModels;
using Point = System.Drawing.Point;
using GemBox.Document;
using System.IO;
using System.Windows.Xps.Packaging;
using Microsoft.Extensions.FileSystemGlobbing;

namespace WpfAppAITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private FileSystemWatcher _watcher;
        private string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "document.docx");
        private string _tempXpsPath;
        public MainWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            var mainWindowViewMOdel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            DataContext = mainWindowViewMOdel;
            mainWindowViewMOdel.Init(WpfAppCanvas, ScreenImage, mainRTB);
             //Host.SizeChanged += LeftGrid_SizeChanged;
             Icon = new BitmapImage(PathToAppUri($"/{typeof(App).Namespace};component/logo.jpg"));

            LoadDocument(_filePath);
        }

        

        public static Uri PathToAppUri(string relativePath)
        {
            return new Uri("pack://application:,,," + relativePath, UriKind.RelativeOrAbsolute);
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
         
        }

        private void WpfAppCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel)DataContext).DrawElement(sender, e);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            documentViewer.FitToWidth();
        }

        public void LoadDocument(string filePath)
        {
            try
            {
                _filePath = filePath;
                _tempXpsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempDocument.docx");

                // Open document in Read-Only mode
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var document = DocumentModel.Load(stream);

                    // Save as XPS to a temporary file
                    document.Save(_tempXpsPath, SaveOptions.XpsDefault);
                }

                // Load XPS into WPF DocumentViewer
                using (XpsDocument xpsDoc = new XpsDocument(_tempXpsPath, FileAccess.Read))
                {
                    documentViewer.Document = xpsDoc.GetFixedDocumentSequence();
                }
                // Start watching for file changes
                StartWatchingFile(filePath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartWatchingFile(string filePath)
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
            }

            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath),
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Changed += OnDocumentChanged;
            _watcher.EnableRaisingEvents = true;
        }
        private void OnDocumentChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LoadDocument(_filePath);  // Reload the document when changed
            });
        }
    }
}