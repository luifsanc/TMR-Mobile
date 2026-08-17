using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Auth;

public partial class AboutTmrPage : ContentPage
{
    public AboutTmrPage(AboutTmrViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
