using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ProyectosFormPage : ContentPage
{
    public ProyectosFormPage(ProyectosFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
