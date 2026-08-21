using tmr_mobile.ViewModels;
using tmr_mobile.Services;
using tmr_mobile.Views.Dashboard.Models;
using tmr_mobile.Views.TimeReport;


namespace tmr_mobile.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    private readonly NotificacionesPage _notificacionesPage;
    private readonly IUserModuleAccessService _moduleAccessService;

    public DashboardPage(
        DashboardViewModel viewModel,
        NotificacionesPage notificacionesPage,
        IUserModuleAccessService moduleAccessService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _notificacionesPage = notificacionesPage;
        _moduleAccessService = moduleAccessService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (tmr_mobile.Views.Shared.OverlayNavigationState.ConsumePreservation(this))
            return;

        try
        {
            var modulos = await _moduleAccessService.ObtenerModulosAsync();
            if (!modulos.Contains("Dashboard"))
            {
                await Shell.Current.GoToAsync("//TimeReportPage");
                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo validar el acceso al Dashboard: {ex.Message}");
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        if (BindingContext is DashboardViewModel vm)
        {
            vm.CargarDashboardCommand.Execute(null);
        }
    }


    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(_notificacionesPage);
    }

    
}
