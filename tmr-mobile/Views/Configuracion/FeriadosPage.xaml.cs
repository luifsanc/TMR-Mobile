using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class FeriadosPage : ContentPage
{
    private readonly FeriadosConfigViewModel _viewModel;

    public FeriadosPage(FeriadosConfigViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }
}
