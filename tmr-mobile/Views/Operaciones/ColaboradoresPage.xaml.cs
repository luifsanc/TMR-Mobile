using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ColaboradoresPage : ContentPage
{
    public ColaboradoresPage(ColaboradoresViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradoresViewModel vm)
        {
            vm.CargarColaboradoresCommand.Execute(null);
        }
    }
}
