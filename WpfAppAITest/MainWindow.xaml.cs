using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Runtime.CompilerServices;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Point = System.Windows.Point;
using System;
using WpfAppAITest.Helpers;
using WpfAppAITest.ViewModels;
using Panel = System.Windows.Controls.Panel;

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

            var dataContext = new MainWindowViewModel(this, OvajZaSliku, Host);

            DataContext = dataContext;

            Host.SizeChanged += LeftGrid_SizeChanged;
        }

        private void LeftGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).ResizeEmbeddedApp();
        }

        private void LoadApplication(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).LoadExternalApplication();
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            Host.SizeChanged -= LeftGrid_SizeChanged;
        }

        private void TakaeScreenShot(object sender, RoutedEventArgs e)
        {
            ScreenShotHelper.CaptureGridAndSetAsBackground(Host, OvajZaSliku);
        }

        private void OvajZaSliku_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel)DataContext).DrawElement(sender, e);
        }
    }
}