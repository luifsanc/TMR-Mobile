using System;
using Microsoft.Maui.Controls;
using tmr_mobile.ViewModels;
using tmr_mobile.Views.Shared;

namespace tmr_mobile.Views.Home;

public partial class HomePage : ContentPage
{
    public Page? OriginPage { get; set; }

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCloseTapped(object? sender, TappedEventArgs e)
    {
        if (Navigation.ModalStack.Contains(this))
        {
            if (OriginPage is not null)
            {
                OverlayNavigationState.PreserveOnNextAppearing(OriginPage);
            }

            await Navigation.PopModalAsync();
            return;
        }

        await Shell.Current.GoToAsync("//DashboardPage");
    }
}
