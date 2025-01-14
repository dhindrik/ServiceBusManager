﻿using ServiceBusManager.Services;
using ServiceBusManager.ViewModels;

namespace ServiceBusManager.Views;

public partial class ConnectView
{
    private readonly ILogService logService;

    public ConnectView(ConnectViewModel viewModel, ILogService logService)
	{
		InitializeComponent();

		BindingContext = viewModel;
        this.logService = logService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Task.Run(async () => await logService.LogPageView(nameof(ConnectView)));
    }
}
