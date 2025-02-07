using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
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
        private Canvas _canvas;


        public MainWindowViewModel(Window mainWindow, Panel leftGrid, Canvas canvas)
        {
            _mainWindow = mainWindow;
            _leftGrid = leftGrid;
            _canvas = canvas;
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

        private Point _lineStartPoint;  // Početna tačka linije
        private Point _lineEndPoint;    // Krajna tačka linije

        public void DrawElement(object sender, MouseButtonEventArgs e)
        {
            if (IsLineDrawing)
            {
                // Draw line
                // Ako nije postavljena početna tačka, postavi je
                if (_lineStartPoint is { X: 0, Y: 0 })
                {
                    _lineStartPoint = e.GetPosition((UIElement)sender);
                }
                else
                {
                    // Krajna tačka linije
                    _lineEndPoint = e.GetPosition((UIElement)sender);

                    // Nacrtaj liniju
                    Line line = new Line
                    {
                        X1 = _lineStartPoint.X,
                        Y1 = _lineStartPoint.Y,
                        X2 = _lineEndPoint.X,
                        Y2 = _lineEndPoint.Y,
                        Stroke = System.Windows.Media.Brushes.Black,
                        StrokeThickness = 2
                    };

                    // Dodaj liniju na Canvas
                    _canvas.Children.Add(line);

                    _lineStartPoint.X = 0;
                    _lineStartPoint.Y = 0;

                    _lineEndPoint.X = 0;
                    _lineEndPoint.Y = 0;
                }
            }
            else if (IsArrowDrawing)
            {
                if (_lineStartPoint is { X: 0, Y: 0 })
                {
                    _lineStartPoint = e.GetPosition((UIElement)sender);
                }
                else
                {
                    // Krajna tačka strelice
                    _lineEndPoint = e.GetPosition((UIElement)sender);

                    // Nacrtaj osnovnu liniju strelice
                    Line line = new Line
                    {
                        X1 = _lineStartPoint.X,
                        Y1 = _lineStartPoint.Y,
                        X2 = _lineEndPoint.X,
                        Y2 = _lineEndPoint.Y,
                        Stroke = System.Windows.Media.Brushes.Black,
                        StrokeThickness = 2
                    };

                    // Dodaj osnovnu liniju strelice na Canvas
                    _canvas.Children.Add(line);

                    // Nacrtaj vrh strelice
                    DrawArrowHead(line.X1, line.Y1, line.X2, line.Y2);

                    // Resetuj tačke nakon crtanja strelice
                    _lineStartPoint.X = 0;
                    _lineStartPoint.Y = 0;

                    _lineEndPoint.X = 0;
                    _lineEndPoint.Y = 0;
                }
            }
            else if (IsRactangeDrawing)
            {
                // Draw rectangle
            }
        }


        private void DrawArrowHead(double startX, double startY, double endX, double endY)
        {
            double arrowHeadLength = 10; // Dužina vrha strelice
            double arrowHeadAngle = Math.PI / 6; // Ugao vrha strelice (u radijanima)

            // Izračunaj ugao linije
            double angle = Math.Atan2(endY - startY, endX - startX);

            // Izračunaj tačke za vrh strelice
            Point arrowPoint1 = new Point(endX - arrowHeadLength * Math.Cos(angle - arrowHeadAngle),
                endY - arrowHeadLength * Math.Sin(angle - arrowHeadAngle));
            Point arrowPoint2 = new Point(endX - arrowHeadLength * Math.Cos(angle + arrowHeadAngle),
                endY - arrowHeadLength * Math.Sin(angle + arrowHeadAngle));

            // Treća tačka koja se nalazi na suprotnom kraju od vrha
            Point arrowPoint3 = new Point(endX - arrowHeadLength * 0.5 * Math.Cos(angle),
                endY - arrowHeadLength * 0.5 * Math.Sin(angle));

            // Nacrtaj vrh strelice sa tri linije
            Polyline arrowHead = new Polyline
            {
                Points = new PointCollection { new Point(endX, endY), arrowPoint1, arrowPoint3, arrowPoint2 },
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 2
            };

            // Dodaj vrh strelice na Canvas
            _canvas.Children.Add(arrowHead);
        }


        private bool _isLineDrawing;

        public bool IsLineDrawing
        {
            get => _isLineDrawing;
            set
            {
                _isLineDrawing = value;

                if (value == true)
                {
                    IsArrowDrawing = false;
                    IsRactangeDrawing = false;
                }
                OnPropertyChanged(nameof(IsLineDrawing));
            }
        }

        private bool _isArrowDrawing;

        public bool IsArrowDrawing
        {
            get => _isArrowDrawing;
            set
            {
                _isArrowDrawing = value;

                if (value == true)
                {
                    IsLineDrawing = false;
                    IsRactangeDrawing = false;
                }
                OnPropertyChanged(nameof(IsArrowDrawing));
            }
        }

        private bool _isRactangeDrawing;

        public bool IsRactangeDrawing
        {
            get => _isRactangeDrawing;
            set
            {
                _isRactangeDrawing = value;

                if (value == true)
                {
                    IsArrowDrawing = false;
                    IsLineDrawing = false;
                }

                OnPropertyChanged(nameof(IsRactangeDrawing));
            }
        }
    }
}
