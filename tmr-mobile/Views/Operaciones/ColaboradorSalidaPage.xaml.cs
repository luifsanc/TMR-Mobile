using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ColaboradorSalidaPage : ContentPage
{
    public ColaboradorSalidaPage(ColaboradorSalidaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
