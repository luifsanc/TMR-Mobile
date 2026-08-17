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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (tmr_mobile.Views.Shared.OverlayNavigationState.ConsumePreservation(this))
            return;

        if (BindingContext is NotificacionesViewModel viewModel)
        {
            viewModel.CargarNotificacionesCommand.Execute(null);
        }
    }

    private async void OnCloseTapped(object? sender, TappedEventArgs e)
    {
        if (Navigation.ModalStack.Contains(this))
        {
            await Navigation.PopModalAsync();
            return;
        }

        await Shell.Current.GoToAsync("//DashboardPage");
    }
}
