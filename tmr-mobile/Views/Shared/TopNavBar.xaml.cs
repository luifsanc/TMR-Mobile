using Microsoft.Maui.Controls;

namespace tmr_mobile.Views.Shared;

public partial class TopNavBar : ContentView
{
    public TopNavBar()
    {
        InitializeComponent();
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
