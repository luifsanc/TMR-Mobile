using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.TimeReport;

public partial class CrearActividadPage : ContentPage
{
    public CrearActividadPage(CrearActividadViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CrearActividadViewModel vm)
        {
            vm.CargarDatosCommand.Execute(null);
        }
    }
}
