using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class UsuariosPage : ContentPage
{
    private readonly UsuariosConfigViewModel _viewModel;

    public UsuariosPage(UsuariosConfigViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        System.Diagnostics.Debug.WriteLine("[USUARIOS] Inicio página");
        await _viewModel.InicializarAsync();
    }
}
