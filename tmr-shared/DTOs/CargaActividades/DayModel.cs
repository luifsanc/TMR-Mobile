using CommunityToolkit.Mvvm.ComponentModel;

namespace tmr_mobile.Models;

public partial class DayModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DayNumber))]
    private DateTime _date;

    public int DayNumber => Date.Day;

    [ObservableProperty]
    private bool _isCurrentMonth;

    [ObservableProperty]
    private bool _isToday;

    [ObservableProperty]
    private bool _hasActivities;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string _specialDayText = string.Empty;
}
