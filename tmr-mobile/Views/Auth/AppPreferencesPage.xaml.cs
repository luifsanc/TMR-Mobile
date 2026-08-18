using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Auth;

public partial class AppPreferencesPage : ContentPage
{
    public AppPreferencesPage(AppPreferencesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
