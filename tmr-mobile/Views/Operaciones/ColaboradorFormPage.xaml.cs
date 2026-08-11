using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ColaboradorFormPage : ContentPage
{
    public ColaboradorFormPage(ColaboradorFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
