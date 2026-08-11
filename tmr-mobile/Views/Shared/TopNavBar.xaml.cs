using System;
using Microsoft.Maui.Controls;

namespace tmr_mobile.Views.Shared;

public partial class TopNavBar : ContentView
{
    public TopNavBar()
    {
        InitializeComponent();
    }

    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        DropdownMenu.IsVisible = false;
        await Shell.Current.GoToAsync("//dashboard");
    }

    private void OnMenuClicked(object? sender, EventArgs e)
    {
        DropdownMenu.IsVisible = !DropdownMenu.IsVisible;
    }

    private async void OnNavigate(object? sender, TappedEventArgs e)
    {
        DropdownMenu.IsVisible = false;
        if (e.Parameter is string route)
        {
            await Shell.Current.GoToAsync($"//{route}");
        }
    }
}
