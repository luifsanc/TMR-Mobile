using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ClientesPage : ContentPage
{
    public ClientesPage(ClientesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ClientesViewModel vm)
        {
            vm.CargarClientesCommand.Execute(null);
        }
    }
}
