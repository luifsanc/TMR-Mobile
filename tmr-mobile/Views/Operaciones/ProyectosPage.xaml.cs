using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ProyectosPage : ContentPage
{
    public ProyectosPage(ProyectosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProyectosViewModel vm)
        {
            vm.CargarProyectosCommand.Execute(null);
        }
    }
}
