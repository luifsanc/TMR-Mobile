using tmr_mobile.ViewModels;
using tmr_mobile.Models.Configuracion;

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

    private async void OnTipoCatalogoChanged(object? sender, EventArgs e)
    {
        if (sender is not Picker { SelectedItem: CatalogoMaster catalogo } ||
            _viewModel.CatalogoSeleccionado?.Id == catalogo.Id)
        {
            return;
        }

        await _viewModel.SeleccionarCatalogoCommand.ExecuteAsync(catalogo);
    }
}
