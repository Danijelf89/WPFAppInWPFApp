using Microsoft.Extensions.DependencyInjection;
using WpfAppAITest.ViewModels;
using WpfAppAITest.Views;

namespace WpfAppAITest.DependancyInjection
{
    internal static class ServiceExtensions
    {
        public static void InitializeDependencyInjection(this IServiceCollection services)
        {
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<ScreenChooserViewModel>();
            services.AddTransient<ScreenChooserView>();
            services.AddTransient<MainWindow>();

        }
    }
}
