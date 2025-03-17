using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using WpfAppAITest.Command;
using WpfAppAITest.Helpers;
using WpfAppAITest.Models;
using WpfAppAITest.Services;
using WpfAppAITest.Views;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using Line = System.Windows.Shapes.Line;
using Point = System.Windows.Point;
using RichTextBox = System.Windows.Controls.RichTextBox;


namespace WpfAppAITest.ViewModels
{
    public  class MainWindowViewModel : BaseViewModel
    {
        private  DispatcherTimer _timer;
        private  DispatcherTimer _timerHealthCheck;
        private readonly IServiceProvider _serviceProvider;
        private System.Windows.Controls.Image _shareImage;
        private  TranscriptionService _transcriptionService;
        private  AiProcessingService _aiProcessingService;
        private Canvas _canvas;
        private  RichTextBox _richTextBox;

        public MainWindowViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            CheckServerConnection();
            
        }

        public void Init(Canvas canvas, System.Windows.Controls.Image shareImage, RichTextBox rtb)
        {
            _shareImage = shareImage;
            _canvas = canvas;
            _richTextBox = rtb;
            _transcriptionService = _serviceProvider.GetRequiredService<TranscriptionService>();
            _aiProcessingService = _serviceProvider.GetRequiredService<AiProcessingService>();

            _timerHealthCheck = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timerHealthCheck.Tick += async (s, e) => CheckServerConnection();
            _timerHealthCheck.Start();
        }

        private async void CheckServerConnection()
        {
            var healthCheck = _serviceProvider.GetRequiredService<HealthCheckService>();
            var response = await healthCheck.CheckIfAlive();


            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (response)
                {
                    IsOnlineVisible = true;
                    IsOfflineVisible = false;
                }
                else
                {
                    IsOnlineVisible = false;
                    IsOfflineVisible = true;
                }
            }));
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

        private DelegateCommand _clearScreenShot;
        public DelegateCommand CleanScreenShotCommand => _clearScreenShot ??=
            new DelegateCommand(ClearScreenShot, _ => true);

        private DelegateCommand _recordVoice;
        public DelegateCommand RecordVoiceCommand => _recordVoice ??=
            new DelegateCommand(RecordVoice, _ => true);

        private ScreenModel _selectedScreen; // Store selected screen globally

        private void ShareScreen(object o)
        {
            var screenChooserwindow = _serviceProvider.GetRequiredService<ScreenChooserView>();
            screenChooserwindow.Owner = Application.Current.MainWindow;
            screenChooserwindow.ShowDialog();

            if(screenChooserwindow.DialogResult == false) return;

            _selectedScreen = (screenChooserwindow.DataContext as ScreenChooserViewModel).SelectedScreen;
            screenChooserwindow.DataContext = null;


            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick; //CaptureWindowClientArea("msedge")
            _timer.Start();

            LabelVisible = Visibility.Collapsed;
            IsScreenCapturing = true;
        }
        public bool ShareScreenCanExecute() => IsScreenCapturing;

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _shareImage.Source = ScreenCaptureHelper.CaptureScreen(_selectedScreen.Scrren);
        }

        private void TakeScreenshot(object o)
        {
            ScrenshhotLabelVisible = Visibility.Collapsed;
            _canvas.Background = new ImageBrush(_shareImage.Source);
        }

        private void StopScreenShare(object o)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _shareImage.Source = null;
            LabelVisible = Visibility.Visible;
            IsScreenCapturing = false;
        }

        private void ClearScreenShot(object o)
        {
            _canvas.Background = Brushes.Transparent;
            ScrenshhotLabelVisible = Visibility.Visible;

            while(_canvas.Children.Count > 0)
            {
                _canvas.Children.RemoveAt(_canvas.Children.Count - 1);
            }
        }
        private bool _isRecording;
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                _isRecording = value;
                OnPropertyChanged(nameof(IsRecording));
            }
        }

        private bool _isOnlineVisible;
        public bool IsOnlineVisible
        {
            get => _isOnlineVisible;
            set
            {
                _isOnlineVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isOfflineVisible;
        public bool IsOfflineVisible
        {
            get => _isOfflineVisible;
            set
            {
                _isOfflineVisible = value;
                OnPropertyChanged();
            }
        }


        private async void RecordVoice(object o)
        {
            if (!IsRecording)
            {
                _transcriptionService.StartRecording();
                IsRecording = true;
            }
            else
            {
                IsRecording = false;
                _transcriptionService.StopRecording();
                var path = _transcriptionService.GetRecordedFilePath();
                var transcribedText = await _aiProcessingService.TranscribeAudioAsync(path);
                _richTextBox.AppendText(transcribedText);

                List<DocxSection> sections = new();
                sections.Add(new DocxSection
                {
                    Text = transcribedText
                });
                CreateDocument(sections);
            }
        }
        private async void CreateDocument(List<DocxSection> section)
        {
            var doc = await _aiProcessingService.GenerateDocxAsync(section, "this is test");
            var docString = JsonSerializer.Deserialize<DocxResponse>(doc);
            string base64Docx = docString.docx;
            DocumentHelper.SaveDocx(base64Docx);
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
                if (_isScreenCapturing == value) return;
                _isScreenCapturing = value;
                OnPropertyChanged();

                ShareScreenCommand.RaiseCanExecuteChanged();
                AddScreenshot.RaiseCanExecuteChanged();
                DeleteAppCommand.RaiseCanExecuteChanged();
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

                var x = (int)Math.Min(_rectStartPoint.X, _rectEndPoint.X);
                var y = (int)Math.Min(_rectStartPoint.Y, _rectEndPoint.Y);
                var width = (int)Math.Abs(_rectStartPoint.X - _rectEndPoint.X);
                var height = (int)Math.Abs(_rectStartPoint.Y - _rectEndPoint.Y);

                var rect = new System.Windows.Shapes.Rectangle
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

    class DocxResponse
    {
        public string docx { get; set; } = string.Empty;
    }

}
