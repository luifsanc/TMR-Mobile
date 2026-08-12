using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using tmr_mobile.Models;
using tmr_mobile.Services;
using tmr_shared.DTOs.CargaActividades;
using tmr_shared.DTOs.Dashboard;

namespace tmr_mobile.ViewModels;

public partial class CargaActividadesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    // Cache completo sin filtrar. Se descarga una vez y el filtro/búsqueda
    // se resuelve en memoria, para no golpear la API en cada tecla.
    private List<ActividadCargaResponse> _todasLasActividades = new();

    private const int TamañoPagina = 10;
    private List<ActividadCargaResponse> _filtradasCache = new();

    // Selección acumulada entre páginas: Actividades se recrea por completo en cada
    // cambio de página, así que el estado de selección no puede vivir solo en
    // ActividadCargaItem.IsSeleccionado. Se guarda por Id para poder restaurar el
    // check al repintar la página.
    private readonly HashSet<int> _idsSeleccionados = new();

    private static readonly CultureInfo CulturaEc = CultureInfo.GetCultureInfo("es-EC");

    [ObservableProperty]
    public partial string HorasPorRegistrarTexto { get; set; } = "0 h";

    [ObservableProperty]
    public partial string RangoRegistrosTexto { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int PaginaActual { get; set; } = 1;

    public bool PuedeIrAnterior => PaginaActual > 1;
    public bool PuedeIrSiguiente => PaginaActual * TamañoPagina < _filtradasCache.Count;


    [ObservableProperty]
    public partial string ArchivoSeleccionado { get; set; } = "Ningún archivo seleccionado";

    [ObservableProperty]
    public partial string TextoBusqueda { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CampoFiltro { get; set; } = "Colaborador";

    public List<string> CamposFiltroDisponibles { get; } = new() { "Colaborador", "Proyecto", "Cliente" };

    [ObservableProperty]
    public partial string HorasFaltantesTexto { get; set; } = "-";

    [ObservableProperty]
    public partial bool TieneHorasFaltantes { get; set; }

    public bool NoTieneHorasFaltantes => !TieneHorasFaltantes;

    public bool TieneSeleccionActividades => _idsSeleccionados.Count > 0;

    // La descarga la resuelve el backend sobre el total de actividades: el botón no
    // depende de la selección, solo se bloquea mientras hay una descarga en curso.
    public bool PuedeDescargar => !DescargandoArchivo;

    public string TextoBotonDescargar => DescargandoArchivo ? "Generando archivo..." : "Descargar";

    partial void OnTieneHorasFaltantesChanged(bool value)
    {
        OnPropertyChanged(nameof(NoTieneHorasFaltantes));
    }

    [ObservableProperty]
    public partial bool CargandoActividades { get; set; }

    // IsBusy exclusivo del botón Descargar: separado de CargandoActividades porque
    // ese último dispara el ActivityIndicator de card completo del listado principal.
    [ObservableProperty]
    public partial bool DescargandoArchivo { get; set; }

    partial void OnDescargandoArchivoChanged(bool value)
    {
        OnPropertyChanged(nameof(PuedeDescargar));
        OnPropertyChanged(nameof(TextoBotonDescargar));
    }

    public System.Collections.ObjectModel.ObservableCollection<ActividadCargaItem> Actividades { get; } = new();

    public CargaActividadesViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Carga Masiva de Actividades";

        CargarActividadesCommand.Execute(null);
        CargarHorasIncompletasCommand.Execute(null);
    }

    partial void OnPaginaActualChanged(int value)
    {
        OnPropertyChanged(nameof(PuedeIrAnterior));
        OnPropertyChanged(nameof(PuedeIrSiguiente));
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
    }


    partial void OnTextoBusquedaChanged(string value)
    {
        PaginaActual = 1;
        AplicarFiltro();
    }

    partial void OnCampoFiltroChanged(string value)
    {
        PaginaActual = 1;
        AplicarFiltro();
    }

    //partial void OnTextoBusquedaChanged(string value) => AplicarFiltro();
    //partial void OnCampoFiltroChanged(string value) => AplicarFiltro();

    [RelayCommand]
    private async Task CargarActividadesAsync()
    {
        CargandoActividades = true;
        ErrorMessage = string.Empty;
        try
        {
            var response = await _apiService.GetAsync<List<ActividadCargaResponse>>("api/carga-actividades");
            _todasLasActividades = response ?? new List<ActividadCargaResponse>();

            var totalHoras = _todasLasActividades.Sum(a => a.NroHoras);
            HorasPorRegistrarTexto = $"{totalHoras.ToString("N2", CulturaEc)} h";

            AplicarFiltro();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar actividades: {ex.Message}";
        }
        finally
        {
            CargandoActividades = false;
        }
    }

    [RelayCommand]
    private async Task CargarHorasIncompletasAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<HorasIncompletasResponse>("api/dashboard/mis-horas-incompletas?rango=mes");
            if (response != null)
            {
                TieneHorasFaltantes = response.TieneFaltantes;
                HorasFaltantesTexto = response.TieneFaltantes
                    ? $"{response.HorasFaltantes:0.#} h"
                    : "Estás al día";
            }
        }
        catch (Exception ex)
        {
            HorasFaltantesTexto = "—";
            System.Diagnostics.Debug.WriteLine($"[HorasIncompletas] {ex.Message}");
        }
    }


    private enum CriterioOrden { FechaDesc, FechaAsc, ColaboradorAsc, HorasDesc }
    private CriterioOrden _criterioOrden = CriterioOrden.FechaDesc;


    [ObservableProperty]
    public partial string OrdenTexto { get; set; } = "Más recientes";

    [RelayCommand]
    [Obsolete]
    private async Task AbrirFiltrosAsync()
    {
        var opcion = await Shell.Current.DisplayActionSheet(
            "Filtrar por", "Cancelar", null, "Colaborador", "Proyecto", "Cliente");

        if (string.IsNullOrEmpty(opcion) || opcion == "Cancelar") return;

        CampoFiltro = opcion; // ya dispara OnCampoFiltroChanged -> AplicarFiltro
    }

    [RelayCommand]
    [Obsolete]
    private async Task OrdenarAsync()
    {
        var opcion = await Shell.Current.DisplayActionSheet(
            "Ordenar por", "Cancelar", null,
            "Fecha (más recientes)", "Fecha (más antiguas)", "Colaborador (A-Z)", "Horas (mayor a menor)");

        if (string.IsNullOrEmpty(opcion) || opcion == "Cancelar") return;

        _criterioOrden = opcion switch
        {
            "Fecha (más antiguas)" => CriterioOrden.FechaAsc,
            "Colaborador (A-Z)" => CriterioOrden.ColaboradorAsc,
            "Horas (mayor a menor)" => CriterioOrden.HorasDesc,
            _ => CriterioOrden.FechaDesc,
        };

        OrdenTexto = opcion;
        PaginaActual = 1;
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        IEnumerable<ActividadCargaResponse> filtradas = _todasLasActividades;

        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            var termino = TextoBusqueda.Trim();
            filtradas = CampoFiltro switch
            {
                "Proyecto" => filtradas.Where(a => a.Proyecto.Contains(termino, StringComparison.OrdinalIgnoreCase)),
                "Cliente" => filtradas.Where(a => a.Cliente.Contains(termino, StringComparison.OrdinalIgnoreCase)),
                _ => filtradas.Where(a => a.Colaborador.Contains(termino, StringComparison.OrdinalIgnoreCase)),
            };
        }

        filtradas = _criterioOrden switch
        {
            CriterioOrden.FechaAsc => filtradas.OrderBy(a => a.Fecha),
            CriterioOrden.ColaboradorAsc => filtradas.OrderBy(a => a.Colaborador),
            CriterioOrden.HorasDesc => filtradas.OrderByDescending(a => a.NroHoras),
            _ => filtradas.OrderByDescending(a => a.Fecha),
        };

        _filtradasCache = filtradas.ToList();
        MostrarPaginaActual();
    }

    private void MostrarPaginaActual()
    {
        foreach (var item in Actividades)
            item.PropertyChanged -= OnActividadItemPropertyChanged;

        Actividades.Clear();

        var inicio = (PaginaActual - 1) * TamañoPagina;

        foreach (var item in _filtradasCache.Skip(inicio).Take(TamañoPagina))
        {
            var nuevo = new ActividadCargaItem
            {
                Id = item.Id,
                Colaborador = item.Colaborador,
                Proyecto = item.Proyecto,
                Cliente = item.Cliente,
                LiderTecnico = item.LiderTecnico,
                FechaTexto = item.Fecha.ToString("dd/MM/yyyy"),
                Fecha = item.Fecha.ToString("yyyy-MM-dd"),
                HorasTexto = $"{item.NroHoras:0.##} h",
                NroHoras = item.NroHoras,
                Estado = item.Estado,
                IsSeleccionado = _idsSeleccionados.Contains(item.Id)
            };
            nuevo.PropertyChanged += OnActividadItemPropertyChanged;
            Actividades.Add(nuevo);
        }

        RangoRegistrosTexto = _filtradasCache.Count == 0
            ? "Sin resultados"
            : $"{inicio + 1} al {Math.Min(PaginaActual * TamañoPagina, _filtradasCache.Count)} de {_filtradasCache.Count}";

        OnPropertyChanged(nameof(PuedeIrAnterior));
        OnPropertyChanged(nameof(PuedeIrSiguiente));
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
    }

    private void OnActividadItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ActividadCargaItem.IsSeleccionado)) return;
        if (sender is not ActividadCargaItem item) return;

        if (item.IsSeleccionado)
            _idsSeleccionados.Add(item.Id);
        else
            _idsSeleccionados.Remove(item.Id);

        OnPropertyChanged(nameof(TieneSeleccionActividades));
    }

    [RelayCommand(CanExecute = nameof(PuedeIrAnterior))]
    private void PaginaAnterior()
    {
        PaginaActual--;
        MostrarPaginaActual();
    }

    [RelayCommand(CanExecute = nameof(PuedeIrSiguiente))]
    private void PaginaSiguiente()
    {
        PaginaActual++;
        MostrarPaginaActual();
    }


    [RelayCommand]
    [Obsolete]
    private async Task DescargarAsync()
    {
        ErrorMessage = string.Empty;
        DescargandoArchivo = true;
        try
        {
            System.Diagnostics.Debug.WriteLine("[Descargar] 1. Solicitando el archivo al backend...");
            var bytesArchivo = await _apiService.GetFileBytesAsync("api/carga-actividades/download");
            System.Diagnostics.Debug.WriteLine($"[Descargar] 2. Descarga completa, {bytesArchivo?.Length ?? 0} bytes.");

            if (bytesArchivo == null || bytesArchivo.Length == 0)
            {
                ErrorMessage = "No se pudo descargar el archivo de actividades.";
                await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
                return;
            }

            var nombreArchivo = $"actividades-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
            var rutaFinal = await DescargaArchivoHelper.GuardarExcelAsync(bytesArchivo, nombreArchivo);
            System.Diagnostics.Debug.WriteLine($"[Descargar] 3. Guardado: {rutaFinal ?? "(share sheet)"}");

            var mensaje = rutaFinal != null
                ? $"El archivo se guardó en:\n{rutaFinal}"
                : "El archivo está listo. Elige dónde guardarlo.";

            await Shell.Current.DisplayAlert("Archivo descargado", mensaje, "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Descargar] ERROR: {ex}");
            ErrorMessage = $"Error al descargar actividades: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            DescargandoArchivo = false;
        }
    }


        // Propiedad para reflejar en la UI qué archivo se seleccionó (nombre, para mostrarlo si quieres)
[ObservableProperty]
private string? nombreArchivoSeleccionado;

// Guardamos la ruta completa para usarla después (subida, lectura, etc.)
private string? _rutaArchivoSeleccionado;

[RelayCommand]
private async Task SeleccionarArchivoAsync()
{
    try
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                { DevicePlatform.iOS, new[] { "org.openxmlformats.spreadsheetml.sheet" } },
                { DevicePlatform.WinUI, new[] { ".xlsx" } },
                { DevicePlatform.MacCatalyst, new[] { "org.openxmlformats.spreadsheetml.sheet" } },
            });

        var options = new PickOptions
        {
            PickerTitle = "Selecciona un archivo Excel",
            FileTypes = customFileType
        };

        var resultado = await FilePicker.Default.PickAsync(options);

        if (resultado is null)
            return; // el usuario canceló, no es un error

        NombreArchivoSeleccionado = resultado.FileName;
        _rutaArchivoSeleccionado = resultado.FullPath;

        // Aquí después conectamos la subida al backend o el procesamiento local
    }
    catch (Exception ex)
    {
        // TODO: reemplazar con tu manejo de errores habitual (DisplayAlert, logging, etc.)
        Console.WriteLine($"Error al seleccionar archivo: {ex.Message}");
    }
}
}