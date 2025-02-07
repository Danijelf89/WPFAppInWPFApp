using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Interop;
using System.Windows.Threading;
using WpfAppAITest.Helpers;
using MessageBox = System.Windows.Forms.MessageBox;
using Panel = System.Windows.Controls.Panel;
using Point = System.Windows.Point;

namespace WpfAppAITest.ViewModels
{
    public  class MainWindowViewModel : BaseViewModel
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
        const int SW_SHOW = 5;

        private Process _childProcess;
        private IntPtr _childHandle = IntPtr.Zero;

        private Window _mainWindow;
        private Panel _leftGrid;


        public MainWindowViewModel(Window mainWindow, Panel leftGrid)
        {
            _mainWindow = mainWindow;
            _leftGrid = leftGrid;
        }


        public async void LoadExternalApplication()
        {
            LoadingWindow loadingWindow = new LoadingWindow
            {
                Owner = _mainWindow
            };
            loadingWindow.Show();

            var exePath = @"C:\Users\CD-LP000026\Desktop\Workshop\WPF appis\TestPokretanaj DrugeAppUNutarWPF-a\WpfAppAITest\bin\Debug\net8.0-windows\WpfAppAITest.exe";

            _childProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            _childProcess.Start();

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
            SetParent(_childHandle, new WindowInteropHelper(_mainWindow).Handle);

            int style = GetWindowLong(_childHandle, GWL_STYLE);
            SetWindowLong(_childHandle, GWL_STYLE, style | WS_CHILD);

            ResizeEmbeddedApp();

            ShowWindow(_childHandle, SW_SHOW);

            loadingWindow.Close();
            
        }

        public void ResizeEmbeddedApp()
        {
            if (_childHandle != IntPtr.Zero && _leftGrid != null)
            {
                // Dobij apsolutne koordinate LeftGrid-a na ekranu
                Point screenPos = _leftGrid.PointToScreen(new Point(0, 0));

                // Dobij trenutnu veličinu LeftGrid-a
                int width = (int)_leftGrid.ActualWidth;
                int height = (int)_leftGrid.ActualHeight;

                if (width > 0 && height > 0)
                {
                    // Postavi poziciju i veličinu prozora na osnovu LeftGrid-a
                    MoveWindow(_childHandle, (int)screenPos.X, (int)screenPos.Y, width, height, true);
                }
            }
        }
    }
}
