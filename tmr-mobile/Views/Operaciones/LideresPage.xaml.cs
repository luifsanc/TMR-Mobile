using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class LideresPage : ContentPage
{
    public LideresPage(LideresViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LideresViewModel vm)
        {
            vm.CargarLideresCommand.Execute(null);
        }
    }
}
