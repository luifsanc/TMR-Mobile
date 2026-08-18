using System;
using Microsoft.Maui.Controls;
using tmr_mobile.ViewModels;
using tmr_mobile.Views.Shared;

namespace tmr_mobile.Views.Home;

public partial class HomePage : ContentPage
{
    private const double CardGap = 12;
    private double _lastLayoutWidth;

    public Page? OriginPage { get; set; }

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnModulesLayoutSizeChanged(object? sender, EventArgs e)
    {
        UpdateModuleCardSize();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateModuleCardSize();
    }

    private void UpdateModuleCardSize()
    {
        var availableWidth = ModulesLayout.Width;
        if (availableWidth <= 0)
        {
            availableWidth = Width - 40;
        }

        if (availableWidth <= 0 || Math.Abs(availableWidth - _lastLayoutWidth) < 0.5)
        {
            return;
        }

        _lastLayoutWidth = availableWidth;

        var columns = availableWidth switch
        {
            < 240 => 1,
            < 600 => 2,
            < 900 => 3,
            _ => 4
        };

        var cardWidth = Math.Floor((availableWidth - (CardGap * (columns - 1))) / columns);
        var cardHeight = Math.Clamp(cardWidth * 0.72, 96, 132);

        Resources["ModuleCardWidth"] = cardWidth;
        Resources["ModuleCardHeight"] = cardHeight;
        Resources["ModuleIconSize"] = Math.Clamp(cardWidth * 0.2, 26, 36);
        Resources["ModuleTextSize"] = cardWidth < 125 ? 12d : 14d;
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
