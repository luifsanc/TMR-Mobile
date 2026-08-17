using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class RolesPage : ContentPage
{
    private readonly RolesConfigViewModel _viewModel;

    public RolesPage(RolesConfigViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (tmr_mobile.Views.Shared.OverlayNavigationState.ConsumePreservation(this))
            return;

        await _viewModel.InicializarAsync();
    }
}
