﻿global using System.Text.Json;
global using System.Windows.Input;
global using System.Collections.ObjectModel;
global using System.Linq;
global using System.Runtime.CompilerServices; 

global using Microsoft.AspNetCore.Components.WebView.Maui;

global using ServiceBusManager.Models;
global using ServiceBusManager.Services;
global using ServiceBusManager.ViewModels;
global using ServiceBusManager.Views;

global using Azure.Messaging.ServiceBus;
global using Azure.Messaging.ServiceBus.Administration;

global using TinyMvvm;
global using CommunityToolkit.Mvvm;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using CommunityToolkit.Maui;
global using CommunityToolkit.Maui.Alerts;

namespace ServiceBusManager;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fluent-icons.ttf", "Icons");
            })
            .UseTinyMvvm()
            .UseMauiCommunityToolkit()
            .RegisterServices();

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif


        var mauiApp = builder.Build();

		Resolver.Init(mauiApp.Services);

		return mauiApp;
	}

	private static void RegisterServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ILogService, FileLogService>();
        builder.Services.AddSingleton<IPremiumService, PremiumService>();

		builder.Services.AddSingleton<IServiceBusService, ServiceBusService>();

#if DEBUG
		builder.Services.AddSingleton<IConnectionService, DebugConnectionService>();
#else
		builder.Services.AddSingleton<IConnectionService, SecureConnectionService>();
#endif

        builder.Services.AddSingleton<ConnectViewModel>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MessageViewModel>();
        builder.Services.AddSingleton<MessageDetailsViewModel>();
        builder.Services.AddSingleton<InfoViewModel>();
        builder.Services.AddSingleton<ShellViewModel>();
        builder.Services.AddSingleton<PremiumViewModel>();
        builder.Services.AddSingleton<AboutViewModel>();
        builder.Services.AddSingleton<SubscriptionsViewModel>();
        builder.Services.AddSingleton<DeadLettersViewModel>();
        builder.Services.AddSingleton<NewMessageViewModel>();

        builder.Services.AddTransient<PropertyViewModel>();

        builder.Services.AddSingleton<ConnectView>();
        builder.Services.AddSingleton<MainView>();
        builder.Services.AddSingleton<PremiumView>();
        builder.Services.AddSingleton<AboutView>();
        builder.Services.AddSingleton<DeadLettersView>();
    }
}

