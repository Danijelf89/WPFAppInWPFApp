using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAppAITest.Command;
using WpfAppAITest.Models;
using WpfAppAITest.Views;

namespace WpfAppAITest.ViewModels
{
    public class ScreenChooserViewModel : BaseViewModel
    {
        private DispatcherTimer _timer;

        public ScreenChooserViewModel()
        {
            foreach (var scrren in Screen.AllScreens)
            {
                var newScreen = new ScreenModel();
                newScreen.Scrren = scrren;
                newScreen.Name = scrren.DeviceName;
                newScreen.Index = Array.IndexOf(Screen.AllScreens, scrren);
                newScreen.Image = CaptureScreen(scrren);

                Screens.Add(newScreen);
            }

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += (s, e) => UpdateScreenshots();
            _timer.Start();

            if (Screens.Count > 0)
            {
                SelectedScreen = Screens.Count > 0 ? Screens.FirstOrDefault() : null;
            }
        }

        private ObservableCollection<ScreenModel> _screens = new();

        public ObservableCollection<ScreenModel> Screens
        {
            get
            {
                return _screens;
            }
            set
            {
                _screens = value;
                OnPropertyChanged();
            }
        }

        private ScreenModel _selectedScreen;

        public ScreenModel SelectedScreen
        {
            get
            {
                return _selectedScreen;
            }
            set
            {
                _selectedScreen = value;
                OnPropertyChanged();
            }
        }

        private void UpdateScreenshots()
        {
            foreach (var screenModel in Screens)
            {
                screenModel.Image = CaptureScreen(screenModel.Scrren);
                OnPropertyChanged(nameof(Screens)); // Osvježavanje liste
            }
        }
        private BitmapSource CaptureScreen(Screen screen)
        {
            var bounds = screen.Bounds;
            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);

            return ConvertBitmapToImageSource(bitmap);
        }

        private BitmapSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }


        private DelegateCommand _selectWindowCommand;
        public DelegateCommand SelectWindowCommand => _selectWindowCommand ??=
            new DelegateCommand(SelectWindow);

        private void SelectWindow(object o)
        {
            ((o as ScreenChooserView)!).DialogResult = true;
            ((o as ScreenChooserView)!).Close();
        }

        private DelegateCommand _closeWindowCommand;
        public DelegateCommand CloseWindowCommand => _closeWindowCommand ??=
            new DelegateCommand(CloseWindow);

        private void CloseWindow(object o)
        {
            ((o as ScreenChooserView)!).DialogResult = false;
            ((o as ScreenChooserView)!).Close();
        }
    }
}
