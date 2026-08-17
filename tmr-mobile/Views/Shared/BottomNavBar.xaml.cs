using System;
using Microsoft.Maui.Controls;
using tmr_mobile.Views.Home;

namespace tmr_mobile.Views.Shared;

public partial class BottomNavBar : ContentView
{
    private bool _isOpeningMenu;

    public BottomNavBar()
    {
        InitializeComponent();
    }

    private async void OnNavigate(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string route)
        {
            if (route == nameof(HomePage))
            {
                if (_isOpeningMenu)
                {
                    return;
                }

                _isOpeningMenu = true;
                try
                {
                    var menuPage = Handler?.MauiContext?.Services.GetService(typeof(HomePage)) as HomePage;
                    if (menuPage is not null)
                    {
                        menuPage.OriginPage = FindContainingPage();
                        await Navigation.PushModalAsync(menuPage);
                        return;
                    }
                }
                finally
                {
                    _isOpeningMenu = false;
                }
            }

            await Shell.Current.GoToAsync($"//{route}");
        }
    }

    private Page? FindContainingPage()
    {
        Element? element = this;
        while (element is not null)
        {
            if (element is Page page)
            {
                return page;
            }

            element = element.Parent;
        }

        return null;
    }
}
