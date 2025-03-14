using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.Logging;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;
using WpfAppAITest.DependancyInjection;
using Application = System.Windows.Application;

namespace WpfAppAITest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IServiceProvider
    {
        public static IServiceCollection ServiceCollection { get; private set; }
        public static IHost AppHost { get; private set; }


        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<IServiceProvider>(this);
                    services.InitializeDependencyInjection();
                    
                    ServiceCollection = services;
                }).Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        public object? GetService(Type serviceType)
        {
            return AppHost.Services.GetService(serviceType);
        }
    }

}
