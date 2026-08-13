using tmr_mobile.Views.Shared;

namespace tmr_mobile.Services;

public interface IConfirmDialogService
{
    Task<bool> ShowAsync(
        string title,
        string message,
        string confirmText,
        string cancelText,
        string iconText = "!");
}

public sealed class ConfirmDialogService : IConfirmDialogService
{
    public async Task<bool> ShowAsync(
        string title,
        string message,
        string confirmText,
        string cancelText,
        string iconText = "!")
    {
        var dialog = new ConfirmDialogPage(
            title,
            message,
            confirmText,
            cancelText,
            iconText);

        await Shell.Current.Navigation.PushModalAsync(dialog, false);
        return await dialog.Result;
    }
}
