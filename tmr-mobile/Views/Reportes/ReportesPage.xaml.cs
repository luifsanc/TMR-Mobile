using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Reportes;

public partial class ReportesPage : ContentPage
{
    public ReportesPage()
    {
        InitializeComponent();
        BindingContext = new ReportesViewModel();
    }

    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//DashboardPage");
    }
}
