using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Reportes;

public partial class ReporteHorasPage : ContentPage
{
    private readonly ReporteHorasViewModel _viewModel;

    public ReporteHorasPage(ReporteHorasViewModel viewModel)
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
