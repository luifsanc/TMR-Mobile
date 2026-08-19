using tmr_mobile.Services;

namespace tmr_mobile.Views.Shared;

public partial class UserBottomNavBar : ContentView
{
    public UserBottomNavBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            var accessService = Handler?.MauiContext?.Services.GetService<IUserModuleAccessService>();
            var esColaborador = accessService is not null && await accessService.EsColaboradorAsync();
            GeneralNavBar.IsVisible = !esColaborador;
            ColaboradorNavBar.IsVisible = esColaborador;
        }
        catch (Exception ex)
        {
            GeneralNavBar.IsVisible = true;
            ColaboradorNavBar.IsVisible = false;
            System.Diagnostics.Debug.WriteLine($"No se pudo determinar la barra del usuario: {ex.Message}");
        }
    }
}
