using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Auth;

public partial class PersonalDataPage : ContentPage
{
    public PersonalDataPage(PersonalDataViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PersonalDataViewModel viewModel)
            viewModel.LoadCommand.Execute(null);
    }
}
