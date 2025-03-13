
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAppAITest.Command;
using WpfAppAITest.Helpers;
using WpfAppAITest.Models;
using WpfAppAITest.Views;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Drawing.Image;
using Line = System.Windows.Shapes.Line;
using Point = System.Windows.Point;


namespace WpfAppAITest.ViewModels
{
    public  class MainWindowViewModel : BaseViewModel
    {
        private  DispatcherTimer _timer;
        private readonly ScreenCapture _screenCapture = new();

        private System.Windows.Controls.Image _shareImage;

        
        // Import the necessary Windows APIs
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        // Delegate for EnumWindowsProc
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // Method to find the window by process ID
        public static IntPtr FindWindowByProcess(int targetProcessId)
        {
            IntPtr foundWindowHandle = IntPtr.Zero;

            EnumWindows((hWnd, _) =>
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

        public MainWindowViewModel(Canvas canvas, System.Windows.Controls.Image shareImage)
        {
            _canvas = canvas;
            _shareImage = shareImage;
        }

        private DelegateCommand _shareScreenCommand;
        public DelegateCommand ShareScreenCommand => _shareScreenCommand ??=
            new DelegateCommand(ShareScreen, _ => !ShareScreenCanExecute());

        private DelegateCommand _undoCommand;
        public DelegateCommand UndoCommand => _undoCommand ??=
            new DelegateCommand(Undo, _ => true);


        private DelegateCommand _addScreenshot;
        public DelegateCommand AddScreenshot => _addScreenshot ??=
            new DelegateCommand(TakeScreenshot, _ => ShareScreenCanExecute());

        private DelegateCommand _deleteApp;
        public DelegateCommand DeleteAppCommand => _deleteApp ??=
            new DelegateCommand(StopScreenShare, _ => ShareScreenCanExecute());

        private void ShareScreen(object o)
        {
            var newWindow = new ScreenChooserView();
            newWindow.Owner = Application.Current.MainWindow;
            newWindow.ShowDialog();

            if(newWindow.DialogResult == false) return;

            var selectedScreen = (newWindow.DataContext as ScreenChooserViewModel).SelectedScreen;


            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) => _shareImage.Source = BitmapToImageSource(_screenCapture.CaptureScreen(selectedScreen)); //CaptureWindowClientArea("msedge")
            _timer.Start();

            LabelVisible = Visibility.Collapsed;


            IsScreenCapturing = true;
        }
        public bool ShareScreenCanExecute() => IsScreenCapturing;


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
            //_timer.Stop();
            ScrenshhotLabelVisible = Visibility.Collapsed;
            _canvas.Background = new ImageBrush(_shareImage.Source);
            //ScreenShotHelper.CaptureGridAndSetAsBackground(_shareImage, _canvas);
        }

        private void StopScreenShare(object o)
        {
            _timer.Stop();
            _shareImage.Source = null;
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

        private Visibility _screnshhotLabelVisible = Visibility.Visible; // Default is Visible

        public Visibility ScrenshhotLabelVisible
        {
            get => _screnshhotLabelVisible;
            set
            {
                _screnshhotLabelVisible = value;
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
                    Stroke = Brushes.Red,
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
                    Stroke = Brushes.Red,
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
                Fill = Brushes.Red
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
                    Stroke = Brushes.Red,
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

                if (value)
                {
                    IsArrowDrawing = false;
                    IsRactangeDrawing = false;
                }
                OnPropertyChanged();
            }
        }

        private bool _isArrowDrawing;

        public bool IsArrowDrawing
        {
            get => _isArrowDrawing;
            set
            {
                _isArrowDrawing = value;

                if (value)
                {
                    IsLineDrawing = false;
                    IsRactangeDrawing = false;
                }
                OnPropertyChanged();
            }
        }

        private bool _isRactangeDrawing;

        public bool IsRactangeDrawing
        {
            get => _isRactangeDrawing;
            set
            {
                _isRactangeDrawing = value;

                if (value)
                {
                    IsArrowDrawing = false;
                    IsLineDrawing = false;
                }

                OnPropertyChanged();
            }
        }
    }

    public class ScreenCapture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height,
                                          IntPtr hdcSrc, int xSrc, int ySrc, CopyPixelOperation rop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public int X, Y;
        }

        public Bitmap? CaptureWindowClientArea(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                return null;

            IntPtr hWnd = processes[0].MainWindowHandle;
            if (hWnd == IntPtr.Zero)
                return null;

            if (!GetClientRect(hWnd, out Rect clientRect))
                return null;

            Point clientPoint = new Point { X = clientRect.Left, Y = clientRect.Top };
            ClientToScreen(hWnd, ref clientPoint);

            int width = clientRect.Right - clientRect.Left;
            int height = clientRect.Bottom - clientRect.Top;

            IntPtr hdcWindow = GetDC(hWnd);
            IntPtr hdcMemDc = CreateCompatibleDC(hdcWindow);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcWindow, width, height);
            IntPtr hOld = SelectObject(hdcMemDc, hBitmap);

            //var interopWindow = new WindowInteropHelper(this);
            //IntPtr hwnd = interopWindow.Handle;
            //var picker = new GraphicsCapturePicker();
            //var window = picker.As<IInitializeWithWindow>();
            //window.Initialize(hwnd);
            //var item = await picker.PickSingleItemAsync();

            //BitBlt(hdcMemDC, 0, 0, width, height, hdcWindow, clientPoint.X, clientPoint.Y, CopyPixelOperation.SourceCopy);
            BitBlt(hdcMemDc, 0, 0, width, height, hdcWindow, 0, 0,
       CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            var bmp = Image.FromHbitmap(hBitmap);

            SelectObject(hdcMemDc, hOld);
            DeleteObject(hBitmap);
            DeleteDC(hdcMemDc);
            ReleaseDC(hWnd, hdcWindow);

            return bmp;
        }
        
        public Bitmap CaptureScreen(ScreenModel selectedScreenModel)
        {
            var selectedScreen = selectedScreenModel.Scrren;
            var bounds = selectedScreen.Bounds;
            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);
            }
            return bitmap;
        }
    }
}
