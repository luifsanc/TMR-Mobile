using System;
using Microsoft.Maui.Controls;
using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Home;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
    }
}
