using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class CatalogosPage : ContentPage
{
    private readonly CatalogosConfigViewModel _viewModel;

    public CatalogosPage(
        CatalogosConfigViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (tmr_mobile.Views.Shared.OverlayNavigationState.ConsumePreservation(this))
            return;

        await _viewModel.InicializarAsync();
    }
}
