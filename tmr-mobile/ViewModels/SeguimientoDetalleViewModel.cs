using Microsoft.Maui.Controls;
using tmr_mobile.Models.Seguimiento;

namespace tmr_mobile.ViewModels;

[QueryProperty(nameof(Colaborador), "Colaborador")]
public class SeguimientoDetalleViewModel : BaseViewModel
{
    private SeguimientoColaboradorDto _colaborador;

    public SeguimientoColaboradorDto Colaborador
    {
        get => _colaborador;
        set
        {
            _colaborador = value;
            OnPropertyChanged();
        }
    }

    public Command GoBackCommand { get; }

    public SeguimientoDetalleViewModel()
    {
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }
}
