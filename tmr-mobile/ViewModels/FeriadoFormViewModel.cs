using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class FeriadoFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IFeriadosService _feriadosService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    public partial int? IdFeriado { get; set; }

    [ObservableProperty]
    public partial string NombreFeriado { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime FechaFeriado { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string TipoFeriado { get; set; } = "Nacional";

    [ObservableProperty]
    public partial bool EsRecurrente { get; set; }

    [ObservableProperty]
    public partial string Descripcion { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Activo { get; set; } = true;

    [ObservableProperty]
    public partial string TituloPagina { get; set; } = "Agregar Feriado";

    public bool EsEdicion => IdFeriado.HasValue;
    public string TextoBoton => EsEdicion ? "Guardar cambios" : "Guardar";

    public List<string> TiposFeriado { get; } = new() { "Nacional", "Local", "Religioso" };

    public FeriadoFormViewModel(IFeriadosService feriadosService)
    {
        _feriadosService = feriadosService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Feriado", out var fObj) && fObj is FeriadoItem f)
        {
            IdFeriado = f.Id;
            NombreFeriado = f.NombreFeriado;
            FechaFeriado = f.FechaFeriado;
            TipoFeriado = f.TipoFeriado;
            EsRecurrente = f.EsRecurrente;
            Descripcion = f.Descripcion ?? string.Empty;
            Activo = f.Activo;
            TituloPagina = "Editar Feriado";
            return;
        }

        if (query.TryGetValue("IdFeriado", out var idObj))
        {
            if (idObj is int id) IdFeriado = id;
            else if (idObj is string idText && int.TryParse(idText, out var parsedId)) IdFeriado = parsedId;
            else IdFeriado = null;

            TituloPagina = IdFeriado.HasValue ? "Editar Feriado" : "Agregar Feriado";
        }
        else
        {
            IdFeriado = null;
            TituloPagina = "Agregar Feriado";
        }

        _ = CargarDatosInicialesAsync();
    }

    private async Task CargarDatosInicialesAsync()
    {
        if (!EsEdicion || !IdFeriado.HasValue || IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var fer = await _feriadosService.ObtenerFeriadoPorIdAsync(IdFeriado.Value);
            if (fer is not null)
            {
                NombreFeriado = fer.NombreFeriado;
                FechaFeriado = fer.FechaFeriado;
                TipoFeriado = fer.TipoFeriado;
                EsRecurrente = fer.EsRecurrente;
                Descripcion = fer.Descripcion ?? string.Empty;
                Activo = fer.Activo;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar los datos del feriado.";
            System.Diagnostics.Debug.WriteLine($"[FERIADO-FORM][ERROR] {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(NombreFeriado))
        {
            ErrorMessage = "El nombre del feriado es requerido.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TipoFeriado))
        {
            ErrorMessage = "Debes seleccionar el tipo de feriado.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (EsEdicion)
            {
                var update = new UpdateFeriadoRequest
                {
                    NombreFeriado = NombreFeriado.Trim(),
                    FechaFeriado = FechaFeriado.Date,
                    TipoFeriado = TipoFeriado,
                    EsRecurrente = EsRecurrente,
                    Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
                    Activo = Activo
                };

                var success = await _feriadosService.ActualizarFeriadoAsync(IdFeriado!.Value, update);
                if (!success)
                {
                    ErrorMessage = "No se pudo actualizar el feriado.";
                    return;
                }
            }
            else
            {
                var create = new CreateFeriadoRequest
                {
                    NombreFeriado = NombreFeriado.Trim(),
                    FechaFeriado = FechaFeriado.Date,
                    TipoFeriado = TipoFeriado,
                    EsRecurrente = EsRecurrente,
                    Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim()
                };

                var result = await _feriadosService.CrearFeriadoAsync(create);
                if (result is null)
                {
                    ErrorMessage = "No se pudo crear el feriado.";
                    return;
                }
            }

            await Shell.Current.DisplayAlertAsync(
                "Éxito",
                EsEdicion ? "El feriado se actualizó correctamente." : "El feriado se creó correctamente.",
                "Aceptar");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al guardar el feriado.";
            System.Diagnostics.Debug.WriteLine($"[FERIADO-FORM][ERROR] {ex.Message}");
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
