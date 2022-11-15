﻿using System.Linq;
using System.Text.RegularExpressions;

namespace ServiceBusManager.ViewModels;

public sealed partial class DeadLettersViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    public DeadLettersViewModel(IServiceBusService serviceBusService)
    {
        this.serviceBusService = serviceBusService;
    }

    public override async Task Initialize()
    {
        await base.Initialize();

        try
        {
            IsBusy = true;

            await LoadData();
        }
        catch(Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    private async Task LoadData()
    {
        var items = await serviceBusService.GetDeadLetters();

        List<CollectionGroup<DeadLetterInfo>> groups = new();

        Dictionary<string, List<DeadLetterInfo>> info = new();

        foreach (var item in items)
        {
            if (!info.ContainsKey(item.Key))
            {
                info.Add(item.Key, new List<DeadLetterInfo>());
            }

            var group = info[item.Key];

            foreach (var value in item.Value)
            {
                if (value.Name.Contains("/"))
                {
                    var split = value.Name.Split("/");

                    group.Add(new DeadLetterInfo(split[1], value.Count, split[0]) { Connection = item.Key});
                }
                else
                {
                    group.Add(new DeadLetterInfo(value.Name, value.Count) { Connection = item.Key });
                }
            }
        }

        foreach (var item in info.OrderBy(x => x.Key))
        {
            groups.Add(new CollectionGroup<DeadLetterInfo>(item.Key, item.Value));
        }

        Items = new ObservableCollection<CollectionGroup<DeadLetterInfo>>(groups);
    }

    [ObservableProperty]
    private ObservableCollection<CollectionGroup<DeadLetterInfo>> items = new();

    [RelayCommand]
    public async Task ShowPremium()
    {
        await Navigation.NavigateTo("///Premium");
    }

    [RelayCommand]
    public async Task Show(DeadLetterInfo info)
    {
        await Navigation.NavigateTo($"///{nameof(MainViewModel)}", info);
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await base.Initialize();

        try
        {
            IsBusy = true;

            await LoadData();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }
}
