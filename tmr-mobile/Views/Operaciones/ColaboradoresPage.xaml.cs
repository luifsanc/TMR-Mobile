using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ColaboradoresPage : ContentPage
{
    private readonly ColaboradoresViewModel _viewModel;

    public ColaboradoresPage(ColaboradoresViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }
}
