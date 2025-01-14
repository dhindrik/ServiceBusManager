﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace ServiceBusManager.ViewModels;

public sealed partial class MessageViewModel : ViewModel
{
    private readonly IServiceBusService serviceBusService;

    private List<ServiceBusReceivedMessage> selectedMessages = new List<ServiceBusReceivedMessage>();

    public MessageViewModel(IServiceBusService serviceBusService, ILogService logService) : base(logService)
    {
        this.serviceBusService = serviceBusService;

    }

    [ObservableProperty]
    private string? queueName;

    [ObservableProperty]
    private string? topicName;

    [ObservableProperty]
    private string? displayName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotDeadLetter))]
    private bool isDeadLetter;

    public bool IsNotDeadLetter => !IsDeadLetter;

    [ObservableProperty]
    private bool isSubscription;

    [ObservableProperty]
    private bool hasMessages;

    public bool HasSelectedMessages => selectedMessages.Count > 0;
    public int NumberOfSelectedMessages => selectedMessages.Count;

    [ObservableProperty]
    private ObservableCollection<ServiceBusReceivedMessage> messages = new ObservableCollection<ServiceBusReceivedMessage>();

    public async Task LoadMessages(string queueName, bool showDeadLetter = false, bool isTopicSubscription = false)
    {
        try
        {
            IsDeadLetter = showDeadLetter;
            IsSubscription = isTopicSubscription;

            IsBusy = true;

            DisplayName = queueName;

            if (queueName.Contains("/"))
            {
                var split = queueName.Split("/");

                QueueName = split[1];
                TopicName = split[0];
            }
            else
            {
                TopicName = null;
                QueueName = queueName;
            }

            await LoadAndUpdate();

            selectedMessages.Clear();
            UpdateSelectedMessages();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }


        IsBusy = false;
    }

    private async Task LoadAndUpdate()
    {
        if (QueueName == null)
        {
            return;
        }

        List<ServiceBusReceivedMessage>? messages;

        if (IsDeadLetter)
        {
            messages = await serviceBusService.PeekDeadLetter(QueueName, TopicName);
        }
        else
        {
            messages = await serviceBusService.Peek(QueueName, TopicName);
        }

        await Update(messages);
    }


    private async Task Update(List<ServiceBusReceivedMessage> messages)
    {
        if (!MainThread.IsMainThread)
        {
            await MainThread.InvokeOnMainThreadAsync(() => Update(messages));
            return;
        }

        Messages = new ObservableCollection<ServiceBusReceivedMessage>(messages);
    }

    [RelayCommand]
    private void ShowDetails(ServiceBusReceivedMessage? message)
    {
        if (message == null)
        {
            return;
        }

        (ServiceBusReceivedMessage Message, bool IsDeadLetter, string? TopicName) parameter = (message, IsDeadLetter, TopicName);

        AddAction($"update_messages", () =>
        {
            MainThread.BeginInvokeOnMainThread(async() => await Refresh());
        });

        RunAction($"open_{nameof(MessageDetailsView)}", parameter);
    }

    [RelayCommand]
    private void ToggleMessageSelected(ServiceBusReceivedMessage? message)
    {
        if (message == null)
        {
            return;
        }

        if (selectedMessages.Contains(message))
        {
            selectedMessages.Remove(message);
        }
        else
        {
            selectedMessages.Add(message);
        }

        UpdateSelectedMessages();
    }

    private void UpdateSelectedMessages()
    {
        OnPropertyChanged(nameof(HasSelectedMessages));
        OnPropertyChanged(nameof(NumberOfSelectedMessages));
    }

    [RelayCommand]
    private void ResendMessages()
    {
        if (currentQueueOrTopic == null || !HasSelectedMessages)
        {
            return;
        }

        if(HasNotPremium && selectedMessages.Count > 1)
        {
            RunAction("open_premium");
            return; 
        }


        IsBusy = true;

        Task.Run(async () =>
        {
            try
            {

                foreach (var message in selectedMessages)
                {
                    await serviceBusService.Resend(currentQueueOrTopic, message, TopicName);
                }

                selectedMessages.Clear();
                UpdateSelectedMessages();

                await LoadAndUpdate();

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
        });

    }

    [RelayCommand]
    private void RemoveMessages()
    {
        if (currentQueueOrTopic == null || !HasSelectedMessages)
        {
            return;
        }

        if (HasNotPremium && selectedMessages.Count > 1)
        {
            RunAction("open_premium");
            return;
        }

        IsBusy = true;

        Task.Run(async () =>
        {
            try
            {
                foreach (var message in selectedMessages)
                {
                    await serviceBusService.Remove(currentQueueOrTopic, IsDeadLetter, message, TopicName);
                }

                selectedMessages.Clear();
                UpdateSelectedMessages();

                await LoadAndUpdate();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
        });
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            IsBusy = true;

            selectedMessages.Clear();
            UpdateSelectedMessages();

            await LoadAndUpdate();

        }
        catch(Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    [RelayCommand]
    public void New()
    {
        AddAction($"update_messages", () =>
        {
            MainThread.BeginInvokeOnMainThread(async () => await Refresh());
        });

        RunAction($"open_{nameof(NewMessageView)}");
    }
}

