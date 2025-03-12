using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfAppAITest.Command;
using WpfAppAITest.Models;
using WpfAppAITest.Views;

namespace WpfAppAITest.ViewModels
{
    public class ScreenChooserViewModel : BaseViewModel
    {
        public ScreenChooserViewModel()
        {
            foreach (var scrren in Screen.AllScreens)
            {
                var newScreen = new ScreenModel();
                newScreen.Scrren = scrren;
                newScreen.Name = scrren.DeviceName;
                newScreen.Index = Array.IndexOf(Screen.AllScreens, scrren);

                Screens.Add(newScreen);
            }

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
