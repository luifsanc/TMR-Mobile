using System;
using Microsoft.Maui.Controls;

namespace tmr_mobile.Views.Shared;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
    }

    private async void OnNavigate(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string route)
        {
            await Shell.Current.GoToAsync($"//{route}");
        }
    }
}
