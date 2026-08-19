using tmr_mobile.Views.Home;

namespace tmr_mobile.Views.Shared;

public partial class ColaboradorBottomNavBar : ContentView
{
    private const int LongPressMilliseconds = 650;
    private bool _isOpeningMenu;
    private bool _longPressTriggered;
    private CancellationTokenSource? _pressCancellation;

    public ColaboradorBottomNavBar()
    {
        InitializeComponent();
    }

    private async void OnMainButtonPressed(object? sender, EventArgs e)
    {
        _pressCancellation?.Cancel();
        _pressCancellation?.Dispose();
        _pressCancellation = new CancellationTokenSource();
        _longPressTriggered = false;

        try
        {
            await Task.Delay(LongPressMilliseconds, _pressCancellation.Token);
            _longPressTriggered = true;
            await OpenMenuAsync();
        }
        catch (OperationCanceledException)
        {
            // Se soltó antes del tiempo requerido: se procesa como toque normal.
        }
    }

    private async void OnMainButtonReleased(object? sender, EventArgs e)
    {
        _pressCancellation?.Cancel();
        _pressCancellation?.Dispose();
        _pressCancellation = null;

        if (_longPressTriggered)
        {
            _longPressTriggered = false;
            return;
        }

        await Shell.Current.GoToAsync("//ColaboradorDashboardPage");
    }

    private async Task OpenMenuAsync()
    {
        if (_isOpeningMenu)
            return;

        _isOpeningMenu = true;
        try
        {
            var menuPage = Handler?.MauiContext?.Services.GetService<HomePage>();
            if (menuPage is null)
                return;

            menuPage.OriginPage = FindContainingPage();
            await Navigation.PushModalAsync(menuPage);
        }
        finally
        {
            _isOpeningMenu = false;
        }
    }

    private Page? FindContainingPage()
    {
        Element? element = this;
        while (element is not null)
        {
            if (element is Page page)
                return page;

            element = element.Parent;
        }

        return null;
    }
}
