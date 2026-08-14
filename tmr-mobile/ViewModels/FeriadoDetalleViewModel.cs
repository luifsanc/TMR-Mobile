using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile.ViewModels;

public partial class FeriadoDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IFeriadosService _feriadosService;
    private int _idFeriado;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NombreMostrado))]
    public partial FeriadoItem? Feriado { get; set; }

    public string NombreMostrado => Feriado?.NombreFeriado ?? "Feriado";

    public FeriadoDetalleViewModel(IFeriadosService feriadosService)
    {
        _feriadosService = feriadosService;
        Title = "Detalle del feriado";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Feriado", out var fObj) && fObj is FeriadoItem f)
        {
            Feriado = f;
            _idFeriado = f.Id;
            return;
        }

        if (query.TryGetValue("IdFeriado", out var idObj))
        {
            if (idObj is int id) _idFeriado = id;
            else if (idObj is string idText && int.TryParse(idText, out var parsedId)) _idFeriado = parsedId;
            else return;

            _ = CargarDetalleAsync();
        }
    }

    [RelayCommand]
    private async Task CargarDetalleAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            Feriado = await _feriadosService.ObtenerFeriadoPorIdAsync(_idFeriado);
            if (Feriado is null)
            {
                ErrorMessage = "No se pudo cargar el detalle del feriado.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al cargar el detalle del feriado.";
            System.Diagnostics.Debug.WriteLine($"[FERIADO-DETALLE][ERROR] {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditarAsync()
    {
        if (Feriado is null) return;

        var parameters = new Dictionary<string, object>
        {
            ["IdFeriado"] = Feriado.Id,
            ["Feriado"] = Feriado
        };

        await Shell.Current.GoToAsync(nameof(FeriadoFormPage), parameters);
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (Feriado is null || IsBusy) return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmación",
            $"¿Deseas eliminar el feriado {Feriado.NombreFeriado}?",
            "Sí",
            "No");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            var success = await _feriadosService.EliminarFeriadoAsync(Feriado.Id);

            if (success)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Feriado eliminado",
                    "El feriado se eliminó correctamente.",
                    "Aceptar");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = "No se pudo eliminar el feriado.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al eliminar el feriado.";
            System.Diagnostics.Debug.WriteLine($"[FERIADO-DETALLE][ERROR] {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegresarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
