﻿using Microsoft.Extensions.DependencyInjection;
using WpfAppAITest.Interfaces;
using WpfAppAITest.Managers;
using WpfAppAITest.Services;
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
            services.AddSingleton<IHttpBuilder,HttpBuilder>();
            services.AddTransient<AiProcessingService>();
            services.AddTransient<TranscriptionService>();
            services.AddSingleton<HealthCheckService>();
            services.AddSingleton<IBusyWindow, BusyWindowService>();
            services.AddSingleton<DocumentationManager>();

            
        }
}
}
