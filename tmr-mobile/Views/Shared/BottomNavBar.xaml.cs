using System;
using Microsoft.Maui.Controls;
using tmr_mobile.Services;
using tmr_mobile.Views.Home;

namespace tmr_mobile.Views.Shared;

public partial class BottomNavBar : ContentView
{
    private bool _isOpeningMenu;

    public BottomNavBar()
    {
        InitializeComponent();
    }

    private async void OnDashboardButtonLoaded(object? sender, EventArgs e)
    {
        try
        {
            var accessService = Handler?.MauiContext?.Services
                .GetService(typeof(IUserModuleAccessService)) as IUserModuleAccessService;

            var modulos = accessService is null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : await accessService.ObtenerModulosAsync();

            DashboardButton.IsVisible = modulos.Contains("Dashboard");
        }
        catch (Exception ex)
        {
            DashboardButton.IsVisible = false;
            System.Diagnostics.Debug.WriteLine($"No se pudo validar el acceso al Dashboard: {ex.Message}");
        }
    }

    private async void OnNavigationButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string route })
            await NavigateAsync(route);
    }

    private async void OnMenuButtonClicked(object? sender, EventArgs e) =>
        await NavigateAsync(nameof(HomePage));

    private async Task NavigateAsync(string route)
    {
        if (route == "DashboardPage" && !DashboardButton.IsVisible)
            return;

        if (route == nameof(HomePage))
        {
            if (_isOpeningMenu)
                return;

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
