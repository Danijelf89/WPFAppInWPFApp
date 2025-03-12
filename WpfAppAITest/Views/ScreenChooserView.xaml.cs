using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAppAITest.ViewModels;

namespace WpfAppAITest.Views
{
    /// <summary>
    /// Interaction logic for ScreenChooserView.xaml
    /// </summary>
    public partial class ScreenChooserView : Window
    {
        public ScreenChooserView()
        {
            InitializeComponent();

            var screenCosserVm = new ScreenChooserViewModel();
            DataContext = screenCosserVm;
        }
    }
}
