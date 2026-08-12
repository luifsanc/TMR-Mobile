using tmr_mobile.ViewModels;
using tmr_mobile.Views.Dashboard.Models;


namespace tmr_mobile.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
        {
            vm.CargarDashboardCommand.Execute(null);
        }
    }

    private async void OnVerReporteCompletoClicked(object sender, EventArgs e)
    {
        // Ajusta la ruta Shell según cómo hayas registrado la página de reportes.
        await Shell.Current.GoToAsync("//ReportesPage");
    }

    private void OnMenuTapped(object? sender, TappedEventArgs e)
    {
        // Ajusta según cómo tengas armada la navegación (Shell flyout, etc.)
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    [Obsolete]
    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is DashboardViewModel vm && !vm.TieneNotificaciones)
        {
            await DisplayAlert("Notificaciones", "✅ Estás al día con tus horas", "Aceptar");
            return;
        }

        // TODO: navegar a la pantalla de notificaciones cuando exista.
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