using System.Windows.Input;

namespace tmr_mobile.Views.Components;

public partial class OptionsPopover : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<string>), typeof(OptionsPopover));

    public static readonly BindableProperty SelectedCommandProperty =
        BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(OptionsPopover));

    public static readonly BindableProperty DismissCommandProperty =
        BindableProperty.Create(nameof(DismissCommand), typeof(ICommand), typeof(OptionsPopover));

    public IEnumerable<string> ItemsSource
    {
        get => (IEnumerable<string>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ICommand SelectedCommand
    {
        get => (ICommand)GetValue(SelectedCommandProperty);
        set => SetValue(SelectedCommandProperty, value);
    }

    public ICommand DismissCommand
    {
        get => (ICommand)GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public OptionsPopover()
    {
        InitializeComponent();
    }
}