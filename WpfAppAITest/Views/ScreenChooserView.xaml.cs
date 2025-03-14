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
using Microsoft.Extensions.DependencyInjection;
using WpfAppAITest.ViewModels;

namespace WpfAppAITest.Views
{
    /// <summary>
    /// Interaction logic for ScreenChooserView.xaml
    /// </summary>
    public partial class ScreenChooserView : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public ScreenChooserView(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            var screenCosserVm = _serviceProvider.GetRequiredService<ScreenChooserViewModel>();
            DataContext = screenCosserVm;
            
        }
    }
}
