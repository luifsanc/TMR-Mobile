using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ClienteDetallePage : ContentPage
{
    private bool _esPrimerDespliegue = true;

    public ClienteDetallePage(ClienteDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (tmr_mobile.Views.Shared.OverlayNavigationState.ConsumePreservation(this))
            return;

        if (_esPrimerDespliegue)
        {
            _esPrimerDespliegue = false;
            return;
        }

        if (BindingContext is ClienteDetalleViewModel vm)
        {
            await vm.CargarDetalleCommand.ExecuteAsync(null);
        }
    }
}
