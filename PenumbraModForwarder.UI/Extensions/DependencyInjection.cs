﻿using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Factory;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.Statistics.Services;
using PenumbraModForwarder.UI.Controllers;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Services;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;
using IRegistryHelper = PenumbraModForwarder.UI.Interfaces.IRegistryHelper;
using RegistryHelper = PenumbraModForwarder.UI.Services.RegistryHelper;

namespace PenumbraModForwarder.UI.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ConvertConfiguration>();
        });
        
        // Register ConfigurationModel as a singleton
        services.AddSingleton<ConfigurationModel>();

        // Services
        services.AddSingleton<ISoundManagerService, SoundManagerService>();
        services.AddSingleton<IAria2Service>(_ => new Aria2Service(AppContext.BaseDirectory));
        services.AddSingleton<IRegistryHelper, RegistryHelper>();
        services.AddSingleton<IFileLinkingService, FileLinkingService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IDownloadManagerService, DownloadManagerService>();
        services.AddSingleton<IWebSocketClient, WebSocketClient>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IXivLauncherService, XivLauncherService>();
        services.AddSingleton<IConfigurationListener, ConfigurationListener>();
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<IStatisticService, StatisticService>();
        services.AddSingleton<IFileSizeService, FileSizeService>();
        services.AddSingleton<IXmaHttpClientFactory, XmaHttpClientFactory>();
        services.AddSingleton<IFileDialogService>(provider =>
        {
            var applicationLifetime = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var mainWindow = applicationLifetime?.MainWindow;

            if (mainWindow == null)
            {
                throw new InvalidOperationException("MainWindow is not initialized.");
            }

            return new FileDialogService(mainWindow);
        });
        services.AddSingleton<IXmaModDisplay, XmaModDisplay>();
        services.AddSingleton<ITrayIconController, TrayIconController>();
        services.AddSingleton<ITrayIconManager, TrayIconManager>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ErrorWindowViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<ModsViewModel>();
        services.AddSingleton<HomeViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ErrorWindowViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<ModsViewModel>();
        services.AddSingleton<HomeViewModel>();
        
        services.SetupLogging();

        return services;
    }
    
    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "UI");
    }
    
    public static void EnableSentryLogging()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        var sentryDns = configuration["SENTRY_DNS"];
        if (string.IsNullOrWhiteSpace(sentryDns))
        {
            Console.WriteLine("No SENTRY_DSN provided. Skipping Sentry enablement.");
            return;
        }

        MergedSentryLogging.MergeSentryLogging(sentryDns, "UI");
    }

    public static void DisableSentryLogging()
    {
        MergedSentryLogging.DisableSentryLogging();
    }
}