using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using WpfAppAITest.DependancyInjection;
using GemBox.Document;

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
            RegisterServices();
            RegisterSerialog();
            RegisterLicencies();
        }

        private void RegisterServices()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<IServiceProvider>(this);
                    services.InitializeDependencyInjection();

                    ServiceCollection = services;
                }).Build();
        }

        private void RegisterSerialog()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("serialog.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private void RegisterLicencies()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
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
