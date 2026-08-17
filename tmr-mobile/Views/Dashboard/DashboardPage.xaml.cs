using tmr_mobile.ViewModels;
using tmr_mobile.Views.Dashboard.Models;
using tmr_mobile.Views.TimeReport;


namespace tmr_mobile.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    private readonly NotificacionesPage _notificacionesPage;

    public DashboardPage(
        DashboardViewModel viewModel,
        NotificacionesPage notificacionesPage)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _notificacionesPage = notificacionesPage;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (tmr_mobile.Views.Shared.OverlayNavigationState.ConsumePreservation(this))
            return;

        if (BindingContext is DashboardViewModel vm)
        {
            vm.CargarDashboardCommand.Execute(null);
        }
    }

    private async void OnVerReporteCompletoClicked(object? sender, EventArgs e)
    {
        // Ajusta la ruta Shell según cómo hayas registrado la página de reportes.
        await Shell.Current.GoToAsync("//ReportesPage");
    }

    private void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        // Ajusta según cómo tengas armada la navegación (Shell flyout, etc.)
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(_notificacionesPage);
    }

    [Obsolete]
    private async void OnAvatarTapped(object? sender, TappedEventArgs e)
    {
        const string cambiarContrasena = "Cambiar contraseña";
        const string cerrarSesion = "Cerrar sesión";

        var accion = await DisplayActionSheet(
            title: "Mi cuenta",
            cancel: "Cancelar",
            destruction: cerrarSesion,
            buttons: cambiarContrasena);

        if (accion == cerrarSesion)
        {
            // Navega al login eliminando todo el historial de navegación
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else if (accion == cambiarContrasena)
        {
            // TODO: implementar cambio de contraseña
        }
    }
}
