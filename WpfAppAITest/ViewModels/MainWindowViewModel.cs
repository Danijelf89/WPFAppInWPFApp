using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfAppAITest.Command;
using WpfAppAITest.Helpers;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Windows.Point;

namespace WpfAppAITest.ViewModels
{
    public  class MainWindowViewModel : BaseViewModel
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        private const int SW_SHOW = 5;

        private Process _childProcess;
        private IntPtr _childHandle = IntPtr.Zero;

        //private readonly Panel _leftGrid;
        private readonly Canvas _canvas;
        private WindowsFormsHost _winFormsHost;
        private System.Windows.Forms.Panel _winFormsPanel;


        public MainWindowViewModel(Canvas canvas, WindowsFormsHost winFormsHost)
        {
            _canvas = canvas;
            _winFormsHost = winFormsHost;
            _winFormsPanel = new System.Windows.Forms.Panel();
            _winFormsHost.Child = _winFormsPanel;
        }

        private DelegateCommand _loadAppCommand;
        public DelegateCommand LoadAppCommand => _loadAppCommand ??=
            new DelegateCommand(LoadExternalApplication, _ => true);

        private DelegateCommand _undoCommand;
        public DelegateCommand UndoCommand => _undoCommand ??=
            new DelegateCommand(Undo, _ => true);


        private DelegateCommand _addScreenshot;
        public DelegateCommand AddScreenshot => _addScreenshot ??=
            new DelegateCommand(TakeScreenshot, TakeScreenshotCanExecute);

        private DelegateCommand _deleteApp;
        public DelegateCommand DeleteAppCommand => _deleteApp ??=
            new DelegateCommand(DeleteApp, TakeScreenshotCanExecute);

        private void TakeScreenshot(object o)
        {
            ScreenShotHelper.CaptureGridAndSetAsBackground(_winFormsHost, _canvas);
        }

        private void DeleteApp(object o)
        {
            if (_childHandle != IntPtr.Zero)
            {
                _childProcess.Kill();
                ChildHandle = IntPtr.Zero;
                _winFormsPanel.Controls.Clear();
            }
        }

        private bool TakeScreenshotCanExecute(object o)
        {
            return ChildHandle != IntPtr.Zero;
        }

        public async void LoadExternalApplication(object o)
        {
            var exePath = @"C:\Users\CD-LP000026\Desktop\Workshop\WPF appis\DPiTest\bin\Debug\net8.0-windows\DPiTest.exe";

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
                _childProcess.WaitForInputIdle();
                ChildHandle = _childProcess.MainWindowHandle;
                if (ChildHandle != IntPtr.Zero) break;
                await Task.Delay(200);
            }

            if (_childHandle == IntPtr.Zero)
            {
                MessageBox.Show("Neuspešno dobijanje handle-a prozora aplikacije.");
                return;
            }

            // Postavljamo eksternu aplikaciju kao child od WinForms Panel-a
            SetParent(ChildHandle, _winFormsPanel.Handle);

            // Uklanjamo naslovnu traku prozora
            int style = NativeMethods.GetWindowLong(ChildHandle, NativeMethods.GWL_STYLE);
            NativeMethods.SetWindowLong(_childHandle, NativeMethods.GWL_STYLE, style & ~NativeMethods.WS_CAPTION);

            // Postavimo veličinu aplikacije na veličinu panela
            ResizeEmbeddedApp();

            ShowWindow(ChildHandle, SW_SHOW);
        }

        public void ResizeEmbeddedApp()
        {
            if (_childHandle == IntPtr.Zero) return;
            var width = _winFormsPanel.ClientSize.Width;
            var height = _winFormsPanel.ClientSize.Height;

            if (width > 0 && height > 0)
            {
                MoveWindow(_childHandle, 0, 0, width, height, true);
                _winFormsPanel.Invalidate();
            }
        }

        private Point _lineStartPoint;  // Početna tačka linije
        private Point _lineEndPoint;    // Krajna tačka linije

        public void DrawElement(object sender, MouseButtonEventArgs e)
        {
            if (IsLineDrawing)
            {
                DrawLine(sender, e);
            }
            else if (IsArrowDrawing)
            {
                DrawArrowLine(sender, e);
            }
            else if (IsRactangeDrawing)
            {
                // Draw rectangle
            }
        }

        private void DrawLine(object sender, MouseButtonEventArgs e)
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

        private void DrawArrowLine(object sender, MouseButtonEventArgs e)
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
                    StrokeEndLineCap = PenLineCap.Triangle,
                    StrokeThickness = 2
                };

                // Dodaj osnovnu liniju strelice na Canvas
                _canvas.Children.Add(line);

                // Nacrtaj vrh strelice
                DrawArrowHead(_lineStartPoint, _lineEndPoint);

                // Resetuj tačke nakon crtanja strelice
                _lineStartPoint.X = 0;
                _lineStartPoint.Y = 0;

                _lineEndPoint.X = 0;
                _lineEndPoint.Y = 0;
            }
        }

        public void Undo(object o)
        {
            if (_canvas.Children.Count > 0)
            {
                _canvas.Children.RemoveAt(_canvas.Children.Count - 1);
            }
        }

        private void DrawArrowHead(Point start, Point end)
        {
            // Calculate the direction of the line
            Vector direction = end - start;
            direction.Normalize();  // Normalize to get a unit vector

            // Define the size of the arrowhead
            double arrowSize = 10;

            // Calculate the points of the arrowhead (triangle)
            Point arrowPoint1 = end - direction * arrowSize + new Vector(-direction.Y, direction.X) * 5;
            Point arrowPoint2 = end - direction * arrowSize + new Vector(direction.Y, -direction.X) * 5;

            // Create the Polygon for the arrowhead (triangle)
            Polygon arrowhead = new Polygon
            {
                Points = new PointCollection { arrowPoint1, end, arrowPoint2 },
                Fill = System.Windows.Media.Brushes.Black
            };

            // Add the arrowhead to the Canvas
            _canvas.Children.Add(arrowhead);
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

        public IntPtr ChildHandle
        {
            get => _childHandle;
            set
            {
                _childHandle = value;

                OnPropertyChanged(nameof(ChildHandle));
                AddScreenshot.RaiseCanExecuteChanged();
                DeleteAppCommand.RaiseCanExecuteChanged();
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


        public static class NativeMethods // Promenili smo iz 'internal' u 'public'
        {
            // Za rad sa stilovima prozora
            public const int GWL_STYLE = -16;
            public const int WS_CAPTION = 0x00C00000;

            // P/Invoke za GetWindowLong (dohvatanje stila prozora)
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            // P/Invoke za SetWindowLong (postavljanje stila prozora)
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        }
    }
}
