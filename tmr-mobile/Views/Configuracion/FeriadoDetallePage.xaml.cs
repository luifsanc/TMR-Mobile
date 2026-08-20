using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class FeriadoDetallePage : ContentPage
{
    private bool _esPrimerDespliegue = true;

    public FeriadoDetallePage(FeriadoDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_esPrimerDespliegue)
        {
            _esPrimerDespliegue = false;
            return;
        }

        if (BindingContext is FeriadoDetalleViewModel vm)
        {
            vm.CargarDetalleCommand.Execute(null);
        }
    }
}
