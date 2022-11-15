﻿using Plugin.InAppBilling;
using System;
using System.Threading.Tasks;

namespace ServiceBusManager.ViewModels;

public sealed partial class PremiumViewModel : ViewModel
{
    public PremiumViewModel(IFeatureService featureService)
    {
        this.featureService = featureService;
    }

    public override async Task Initialize()
    {
        await base.Initialize();

        IsBusy = true;
        try
        {
            if (!HasPremium)
            {
                var hasPremium = await RestorePurchase();

                if(!hasPremium)
                {
                    await CrossInAppBilling.Current.ConnectAsync();

                    var lifeItems = await CrossInAppBilling.Current.GetProductInfoAsync(ItemType.InAppPurchase, Constants.Products.Lifetime);

                    LifePrice = lifeItems.First().LocalizedPrice;

                    var items = await CrossInAppBilling.Current.GetProductInfoAsync(ItemType.Subscription, Constants.Products.Monthly, Constants.Products.Yearly);

                    YearPrice = items.Single(x => x.ProductId == Constants.Products.Yearly).LocalizedPrice;
                    MonthPrice = items.Single(x => x.ProductId == Constants.Products.Monthly).LocalizedPrice;
                }
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    [ObservableProperty]
    private string lifePrice;

    [ObservableProperty]
    private string yearPrice;

    [ObservableProperty]
    private string monthPrice;
    private readonly IFeatureService featureService;

    [RelayCommand]
    private async Task BuyLifetime()
    {
        IsBusy = true;

        try
        {

            await CrossInAppBilling.Current.ConnectAsync();

            var result = await CrossInAppBilling.Current.PurchaseAsync(Constants.Products.Lifetime, ItemType.InAppPurchase);

            if (result != null && result.State == PurchaseState.Purchased)
            {
                featureService.AddFeature(Constants.Features.Premium);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task BuyMonthly()
    {
        IsBusy = true;

        try
        {

            await CrossInAppBilling.Current.ConnectAsync();

            var result = await CrossInAppBilling.Current.PurchaseAsync(Constants.Products.Monthly, ItemType.Subscription);

            if (result != null && result.State == PurchaseState.Purchased)
            {
                featureService.AddFeature(Constants.Features.Premium, DateTime.Now.AddMonths(1));
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task BuyYearly()
    {
        IsBusy = true;

        try
        {

            await CrossInAppBilling.Current.ConnectAsync();

            var result = await CrossInAppBilling.Current.PurchaseAsync(Constants.Products.Yearly, ItemType.Subscription);

            if (result != null && result.State == PurchaseState.Purchased)
            {
                featureService.AddFeature(Constants.Features.Premium, DateTime.Now.AddYears(1));
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        IsBusy = false;
    }



    private async Task<bool> RestorePurchase()
    {
        var connected = await CrossInAppBilling.Current.ConnectAsync();

        if (!connected)
        {
            return false;
        }

        var purchases = await CrossInAppBilling.Current.GetPurchasesAsync(ItemType.Subscription);

        if (purchases.Any())
        {


            var purchase = purchases.OrderByDescending(x => x.TransactionDateUtc).FirstOrDefault();

            if (purchase != null)
            {
                if(purchase.ProductId == Constants.Products.Monthly && purchase.TransactionDateUtc < DateTime.UtcNow.AddMonths(1))
                {
                    featureService.AddFeature(Constants.Features.Premium, purchase.TransactionDateUtc.AddMonths(1));
                    return true;
                }
                else if (purchase.ProductId == Constants.Products.Yearly && purchase.TransactionDateUtc < DateTime.UtcNow.AddYears(1))
                {
                    featureService.AddFeature(Constants.Features.Premium, purchase.TransactionDateUtc.AddYears(1));
                    return true;
                }

            }
        }

        purchases = await CrossInAppBilling.Current.GetPurchasesAsync(ItemType.InAppPurchase);

        if (purchases.Any())
        {
            var purchase = purchases.OrderByDescending(x => x.TransactionDateUtc).FirstOrDefault();

            if (purchase != null && purchase.ProductId == Constants.Products.Lifetime)
            {
                featureService.AddFeature(Constants.Features.Premium);
                return true;
            }
        }

            return false;

    }
}

