using tmr_mobile.ViewModels;
using tmr_mobile.Views.TimeReport;

namespace tmr_mobile.Views.Dashboard;

public partial class ColaboradorDashboardPage : ContentPage
{
    private readonly NotificacionesPage _notificacionesPage;

    public ColaboradorDashboardPage(
        ColaboradorDashboardViewModel viewModel,
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

        if (BindingContext is ColaboradorDashboardViewModel viewModel)
            viewModel.CargarCommand.Execute(null);
    }

    private async void OnProfileTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//ProfilePage");
    }

    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(_notificacionesPage);
    }
}
