using Microsoft.Maui.Controls;
using tmr_mobile.Services;

namespace tmr_mobile.Views.Shared;

public partial class TopNavBar : ContentView
{
    public TopNavBar()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        _ = ConfigurarAccesoCargaActividadesAsync();
    }

    private async Task ConfigurarAccesoCargaActividadesAsync()
    {
        var service = Handler?.MauiContext?.Services.GetService(typeof(IUserModuleAccessService))
            as IUserModuleAccessService;

        if (service is null)
        {
            return;
        }

        var esColaborador = await service.EsColaboradorAsync();
        CargaActividadesMenuItem.IsVisible = !esColaborador;
        CargaActividadesMenuSeparator.IsVisible = !esColaborador;
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
