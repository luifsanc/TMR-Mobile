using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public partial class AboutTmrViewModel : BaseViewModel
{
    public AboutTmrViewModel() => Title = "Acerca de TMR";

    public string Version => $"Versión {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";

    [RelayCommand]
    private Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
