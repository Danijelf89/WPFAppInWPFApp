using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfAppAITest.ViewModels;

namespace WpfAppAITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dataContext = new MainWindowViewModel(OvajZaSliku, Host);
            DataContext = dataContext;
            Host.SizeChanged += LeftGrid_SizeChanged;
            Icon = new BitmapImage(PathToAppUri($"/{typeof(App).Namespace};component/logo.jpg"));


        }

        public static Uri PathToAppUri(string relativePath)
        {
            return new Uri("pack://application:,,," + relativePath, UriKind.RelativeOrAbsolute);
        }

        private void LeftGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).ResizeEmbeddedApp();
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            Host.SizeChanged -= LeftGrid_SizeChanged;
        }

        private void OvajZaSliku_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel)DataContext).DrawElement(sender, e);
        }
    }
}