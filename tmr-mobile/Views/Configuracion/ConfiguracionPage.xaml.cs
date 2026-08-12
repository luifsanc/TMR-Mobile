using Microsoft.Maui.Controls;
using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Configuracion;

public partial class ConfiguracionPage : ContentPage
{
    public ConfiguracionPage(ConfiguracionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
