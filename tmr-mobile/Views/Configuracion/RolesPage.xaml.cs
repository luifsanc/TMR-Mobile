using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class RolesPage : ContentPage
{
    public RolesPage(RolesConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
