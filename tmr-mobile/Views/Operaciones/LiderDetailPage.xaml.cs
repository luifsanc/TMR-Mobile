using tmr_shared.DTOs.Lideres;

namespace tmr_mobile.Views.Operaciones;

public partial class LiderDetailPage : ContentPage
{
    public LiderDetailPage(LiderResponse lider)
    {
        InitializeComponent();
        BindingContext = lider;
    }

    private async void OnCerrarClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
