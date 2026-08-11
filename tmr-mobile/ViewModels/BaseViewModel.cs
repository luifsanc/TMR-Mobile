using CommunityToolkit.Mvvm.ComponentModel;

namespace tmr_mobile.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public bool IsNotBusy => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }
}