using tmr_mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace tmr_mobile.Views.Auth;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
