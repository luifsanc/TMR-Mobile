using tmr_mobile.ViewModels;

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
}
