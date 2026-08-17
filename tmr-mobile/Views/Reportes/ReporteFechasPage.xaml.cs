using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Reportes;

public partial class ReporteFechasPage : ContentPage
{
    private readonly ReporteFechasViewModel _viewModel;

    public ReporteFechasPage(ReporteFechasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (tmr_mobile.Views.Shared.OverlayNavigationState.ConsumePreservation(this))
            return;

        await _viewModel.CargarReporteCommand.ExecuteAsync(null);
    }
}
