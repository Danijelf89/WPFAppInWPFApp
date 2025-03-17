using System.Windows;
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
