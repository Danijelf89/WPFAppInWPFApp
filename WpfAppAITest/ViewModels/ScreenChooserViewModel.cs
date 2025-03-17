using System.Collections.ObjectModel;
using System.Windows.Threading;
using WpfAppAITest.Command;
using WpfAppAITest.Helpers;
using WpfAppAITest.Models;
using WpfAppAITest.Views;

namespace WpfAppAITest.ViewModels
{
    public class ScreenChooserViewModel : BaseViewModel
    {
        private DispatcherTimer? _timer;

        public ScreenChooserViewModel()
        {
            foreach (var scrren in Screen.AllScreens)
            {
                var newScreen = new ScreenModel
                {
                    Scrren = scrren,
                    Name = scrren.DeviceName,
                    Index = Array.IndexOf(Screen.AllScreens, scrren),
                    Image = ScreenCaptureHelper.CaptureScreen(scrren)
                };

                Screens.Add(newScreen);
            }

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
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
                screenModel.Image = ScreenCaptureHelper.CaptureScreen(screenModel.Scrren);
                OnPropertyChanged(nameof(Screens)); // Osvježavanje liste
            }
        }
        
        private DelegateCommand _selectWindowCommand;
        public DelegateCommand SelectWindowCommand => _selectWindowCommand ??=
            new DelegateCommand(SelectWindow);

        private void SelectWindow(object o)
        {
            ((o as ScreenChooserView)!).DialogResult = true;
            ((o as ScreenChooserView)!).Close();
            StopSubscriptionOnSelect();
        }

        private DelegateCommand _closeWindowCommand;
        public DelegateCommand CloseWindowCommand => _closeWindowCommand ??=
            new DelegateCommand(CloseWindow);

        private void CloseWindow(object o)
        {
            ((o as ScreenChooserView)!).DialogResult = false;
            ((o as ScreenChooserView)!).Close();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            UpdateScreenshots();
        }

        public void StopSubscriptionOnSelect()
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }
}
