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
        public MainWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
            var mainWindowViewMOdel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            DataContext = mainWindowViewMOdel;
            mainWindowViewMOdel.Init(WpfAppCanvas, ScreenImage, mainRTB);
             Icon = new BitmapImage(PathToAppUri($"/{typeof(App).Namespace};component/logo.jpg"));
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

       
    }
}