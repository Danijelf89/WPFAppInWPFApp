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

namespace WpfAppAITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        const int GWL_STYLE = -16;
        const int WS_CHILD = 0x40000000;
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private Process _childProcess;
        private IntPtr _childHandle = IntPtr.Zero;

        public MainWindow()
        {
            InitializeComponent();
            LeftGrid.SizeChanged += LeftGrid_SizeChanged;
        }


        private async void LoadExternalApplication()
        {
            Window1 loadingWindow = new Window1();
            loadingWindow.Owner = this;
            loadingWindow.Show();

            string exePath = @"C:\Users\CD-LP000026\Desktop\Workshop\WPF appis\TestPokretanaj DrugeAppUNutarWPF-a\WpfAppAITest\bin\Debug\net8.0-windows\WpfAppAITest.exe";

            _childProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            _childProcess.Start();

            //await Task.Delay(1000);

            for (int i = 0; i < 10; i++)
            {
                _childHandle = _childProcess.MainWindowHandle;
                if (_childHandle != IntPtr.Zero) break;
                await Task.Delay(300);
            }

            if (_childHandle == IntPtr.Zero)
            {
                MessageBox.Show("Neuspešno dobijanje handle-a prozora aplikacije.");
                return;
            }

            Dispatcher.Invoke(() =>
            {
                SetParent(_childHandle, new WindowInteropHelper(this).Handle);

                int style = GetWindowLong(_childHandle, GWL_STYLE);
                SetWindowLong(_childHandle, GWL_STYLE, style | WS_CHILD);

                ResizeEmbeddedApp();

                ShowWindow(_childHandle, SW_SHOW);

                // 🔹 2. Zatvori modalni prozor kada je aplikacija spremna
                loadingWindow.Close();
            });
        }

        private void LeftGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeEmbeddedApp();
        }

        private void ResizeEmbeddedApp()
        {
            if (_childHandle != IntPtr.Zero)
            {
                MoveWindow(_childHandle, 0, 0, 1000, 1000, true);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            LoadExternalApplication();
        }
    }
}