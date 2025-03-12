
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAppAITest.Command;
using Brushes = System.Windows.Media.Brushes;
using Line = System.Windows.Shapes.Line;
using Point = System.Windows.Point;

namespace WpfAppAITest.ViewModels
{
    public  class MainWindowViewModel : BaseViewModel
    {
        private  DispatcherTimer _timer;
        private readonly ScreenCapture _screenCapture = new();

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        // Import the necessary Windows APIs
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        // Delegate for EnumWindowsProc
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // Method to find the window by process ID
        public static IntPtr FindWindowByProcess(int targetProcessId)
        {
            IntPtr foundWindowHandle = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                uint processId;
                GetWindowThreadProcessId(hWnd, out processId);

                // Check if the process ID matches the target process ID
                if (processId == targetProcessId)
                {
                    foundWindowHandle = hWnd;
                    return false; // Stop the enumeration if we find the window
                }

                return true; // Continue searching
            }, IntPtr.Zero);

            return foundWindowHandle;
        }

        // Method to find a process by name
        public static int GetProcessIdByName(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                return process.Id;
            }
            return -1; // Not found
        }

        private readonly Canvas _canvas;

        public MainWindowViewModel(Canvas canvas)
        {
            _canvas = canvas;

            foreach (var window in GetOpenWindows())
            {
                var wind = new ShareWindow
                {
                    Handle = window.Handle,
                    Title = window.Title
                };
                OpenWindows.Add(wind);
            }
        }

        private DelegateCommand _shareScreenCommand;
        public DelegateCommand ShareScreenCommand => _shareScreenCommand ??=
            new DelegateCommand(obj => ShareScreen((IntPtr)obj), _ => !ShareScreenCanExecute());


        private DelegateCommand _undoCommand;
        public DelegateCommand UndoCommand => _undoCommand ??=
            new DelegateCommand(Undo, _ => true);


        private DelegateCommand _addScreenshot;
        public DelegateCommand AddScreenshot => _addScreenshot ??=
            new DelegateCommand(TakeScreenshot, _ => ShareScreenCanExecute());

        private DelegateCommand _deleteApp;
        public DelegateCommand DeleteAppCommand => _deleteApp ??=
            new DelegateCommand(StopScreenShare, _ => ShareScreenCanExecute());


        public void ShareScreen(IntPtr selectedWindow)
        {
            if (SelectedWindow.Handle == IntPtr.Zero) return;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += (s, e) => _canvas.Background = new ImageBrush(BitmapToImageSource(_screenCapture.CaptureWindow(SelectedWindow.Handle)));
            _timer.Start();

            LabelVisible = Visibility.Collapsed;
            IsScreenCapturing = true;
        }

        public bool ShareScreenCanExecute() => IsScreenCapturing;

        public static List<(IntPtr Handle, string Title)> GetOpenWindows()
        {
            var windows = new List<(IntPtr, string)>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    StringBuilder title = new StringBuilder(256);
                    GetWindowText(hWnd, title, title.Capacity);
                    if (title.Length > 0)
                        windows.Add((hWnd, title.ToString()));
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }
        private ObservableCollection<ShareWindow> _openedwindows = new();

        public ObservableCollection<ShareWindow> OpenWindows
        {
            get
            {
                return _openedwindows;
            }
            set
            {
                _openedwindows = value;
                OnPropertyChanged(nameof(OpenWindows));
            }
        }
        private ShareWindow _selectedWindow;
        public ShareWindow SelectedWindow
        {
            get => _selectedWindow;
            set
            {
                _selectedWindow = value;
                if (value.Handle != null)
                {
                    ShareScreen(value.Handle);
                }


            }
        }
        private BitmapSource BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void TakeScreenshot(object o)
        {
            _timer.Stop();
        }

        private void StopScreenShare(object o)
        {
            _timer.Stop();
            _canvas.Background = Brushes.Transparent;
            LabelVisible = Visibility.Visible;

            IsScreenCapturing = false;
        }

        private Visibility _labelVisible = Visibility.Visible; // Default is Visible

        public Visibility LabelVisible
        {
            get => _labelVisible;
            set
            {
                _labelVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isScreenCapturing;
        public bool IsScreenCapturing
        {
            get => _isScreenCapturing;
            set
            {
                if (_isScreenCapturing != value)
                {
                    _isScreenCapturing = value;
                    OnPropertyChanged();

                    ShareScreenCommand.RaiseCanExecuteChanged();
                    AddScreenshot.RaiseCanExecuteChanged();
                    DeleteAppCommand.RaiseCanExecuteChanged();
                }
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
                DrawRectangle(sender, e);
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
                    Stroke = System.Windows.Media.Brushes.Red,
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
                    Stroke = System.Windows.Media.Brushes.Red,
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
                Fill = System.Windows.Media.Brushes.Red
            };

            // Add the arrowhead to the Canvas
            _canvas.Children.Add(arrowhead);
        }

        private Point _rectStartPoint;  // Početna tačka
        private Point _rectEndPoint;    // Krajna tačka
        private void DrawRectangle(object sender, MouseButtonEventArgs e)
        {
            if (_rectStartPoint is { X: 0, Y: 0 })
            {
                _rectStartPoint = e.GetPosition((UIElement)sender);
            }
            else
            {
                _rectEndPoint = e.GetPosition((UIElement)sender);

                int x = (int)Math.Min(_rectStartPoint.X, _rectEndPoint.X);
                int y = (int)Math.Min(_rectStartPoint.Y, _rectEndPoint.Y);
                int width = (int)Math.Abs(_rectStartPoint.X - _rectEndPoint.X);
                int height = (int)Math.Abs(_rectStartPoint.Y - _rectEndPoint.Y);

                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                {
                    Width = width,
                    Height = height,
                    Stroke = System.Windows.Media.Brushes.Red,
                    StrokeThickness = 2
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                _canvas.Children.Add(rect);

                _rectStartPoint.X = 0;
                _rectStartPoint.Y = 0;
                _rectEndPoint.X = 0;
                _rectEndPoint.Y = 0;
            }
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

    public class ScreenCapture
    {
        public Bitmap CaptureWindow(IntPtr hwnd)

        {

            RECT rect;

            if (!DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out rect, Marshal.SizeOf(typeof(RECT))))

            {

                GetWindowRect(hwnd, out rect);

            }

            int width = rect.Right - rect.Left;

            int height = rect.Bottom - rect.Top;

            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics gfx = Graphics.FromImage(bitmap))

            {

                IntPtr hdcBitmap = gfx.GetHdc();

                bool success = PrintWindow(hwnd, hdcBitmap, 0);

                gfx.ReleaseHdc(hdcBitmap);

                if (!success)

                {

                    using (Graphics screenGfx = Graphics.FromHwnd(hwnd))

                    {

                        screenGfx.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(width, height));

                    }

                }

            }

            return bitmap;

        }

        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        [DllImport("dwmapi.dll")]

        private static extern bool DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]

        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll")]

        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        private struct RECT

        {

            public int Left, Top, Right, Bottom;

        }

        public Bitmap CaptureScreen()
        {
            var bounds = Screen.AllScreens.LastOrDefault().Bounds;
            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);
            }
            return bitmap;
        }

    }

    public class ShareWindow
    {
        public string Title { get; set; }

        public IntPtr Handle { get; set; }
    }
}
