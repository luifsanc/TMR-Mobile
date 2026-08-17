using tmr_mobile.ViewModels;
using Microsoft.Maui.Controls;
using tmr_mobile.Views.TimeReport;

namespace tmr_mobile.Views.Auth;

public partial class ProfilePage : ContentPage
{
    private readonly NotificacionesPage _notificacionesPage;

    public ProfilePage(
        ProfileViewModel viewModel,
        NotificacionesPage notificacionesPage)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _notificacionesPage = notificacionesPage;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (tmr_mobile.Views.Shared.OverlayNavigationState.ConsumePreservation(this))
            return;

        if (BindingContext is ProfileViewModel viewModel)
        {
            viewModel.CargarEstadoNotificacionesCommand.Execute(null);
        }
    }

    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(_notificacionesPage);
    }
}
