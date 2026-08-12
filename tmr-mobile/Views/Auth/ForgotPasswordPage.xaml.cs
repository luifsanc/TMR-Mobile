using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Auth;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage(
        ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}