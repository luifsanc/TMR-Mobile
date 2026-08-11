using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ClienteFormPage : ContentPage
{
    public ClienteFormPage(ClienteFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
