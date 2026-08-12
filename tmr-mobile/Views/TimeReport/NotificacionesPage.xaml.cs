using Microsoft.Maui.Controls;
using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.TimeReport;

public partial class NotificacionesPage : ContentPage
{
    public NotificacionesPage(NotificacionesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
