using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Auth;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage(ChangePasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is ChangePasswordViewModel { IsRequired: true })
            return true;

        return base.OnBackButtonPressed();
    }
}
