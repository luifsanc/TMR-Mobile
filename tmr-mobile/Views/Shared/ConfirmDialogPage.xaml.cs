namespace tmr_mobile.Views.Shared;

public partial class ConfirmDialogPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _completion = new();
    private bool _isClosing;

    public Task<bool> Result => _completion.Task;

    public ConfirmDialogPage(
        string title,
        string message,
        string confirmText,
        string cancelText,
        string iconText)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        MessageLabel.Text = message;
        ConfirmButton.Text = confirmText;
        CancelButton.Text = cancelText;
        IconLabel.Text = iconText;
    }

    protected override bool OnBackButtonPressed()
    {
        _ = CompleteAsync(false);
        return true;
    }

    private async void OnCancelClicked(object? sender, EventArgs e) =>
        await CompleteAsync(false);

    private async void OnConfirmClicked(object? sender, EventArgs e) =>
        await CompleteAsync(true);

    private async Task CompleteAsync(bool result)
    {
        if (_isClosing)
            return;

        _isClosing = true;
        ConfirmButton.IsEnabled = false;
        CancelButton.IsEnabled = false;

        await Navigation.PopModalAsync(false);
        _completion.TrySetResult(result);
    }
}
