using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using TMR.Shared.DTOs.CargaActividades;
using tmr_mobile.Models;
using tmr_mobile.Services;
using tmr_shared.DTOs.CargaActividades;

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
        //CargarHorasIncompletasCommand.Execute(null);
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
            var response = await _apiService.GetAsync<List<ActividadCargaResponse>>("carga-actividades");
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

    private enum CriterioOrden { FechaDesc, FechaAsc, ColaboradorAsc, HorasDesc }
    private CriterioOrden _criterioOrden = CriterioOrden.FechaDesc;

    [ObservableProperty]
    public partial bool FiltroFechaActivo { get; set; } = false;

    [ObservableProperty]
    public partial DateTime FechaDesde { get; set; } = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    public partial DateTime FechaHasta { get; set; } = DateTime.Today;

    partial void OnFiltroFechaActivoChanged(bool value)
    {
        PaginaActual = 1;
        AplicarFiltro();
    }

    partial void OnFechaDesdeChanged(DateTime value)
    {
        if (FechaDesde > FechaHasta) FechaHasta = FechaDesde; // evita rango invertido
        if (FiltroFechaActivo) { PaginaActual = 1; AplicarFiltro(); }
    }

    partial void OnFechaHastaChanged(DateTime value)
    {
        if (FechaHasta < FechaDesde) FechaDesde = FechaHasta;
        if (FiltroFechaActivo) { PaginaActual = 1; AplicarFiltro(); }
    }

    [ObservableProperty]
    public partial string OrdenTexto { get; set; } = "Más recientes";



    // ===== Filtro (Colaborador / Proyecto / Cliente) =====
    [ObservableProperty]
    public partial bool IsFiltroSelectorVisible { get; set; } = false;

    [RelayCommand]
    private void AbrirFiltros()
    {
        IsFiltroSelectorVisible = true;
    }

    [RelayCommand]
    private void CerrarFiltroSelector()
    {
        IsFiltroSelectorVisible = false;
    }

    [RelayCommand]
    private void SeleccionarFiltro(string opcion)
    {
        IsFiltroSelectorVisible = false;

        if (string.IsNullOrEmpty(opcion))
            return;

        CampoFiltro = opcion; // ya dispara OnCampoFiltroChanged -> AplicarFiltro
    }


    // ===== Ordenar =====
    [ObservableProperty]
    public partial bool IsOrdenSelectorVisible { get; set; } = false;

    public List<string> OpcionesOrdenDisponibles { get; } = new()
    {
        "Fecha (más recientes)", "Fecha (más antiguas)", "Colaborador (A-Z)", "Horas (mayor a menor)"
    };

    [RelayCommand]
    private void AbrirOrden()
    {
        IsOrdenSelectorVisible = true;
    }

    [RelayCommand]
    private void CerrarOrdenSelector()
    {
        IsOrdenSelectorVisible = false;
    }

    [RelayCommand]
    private void SeleccionarOrden(string opcion)
    {
        IsOrdenSelectorVisible = false;

        if (string.IsNullOrEmpty(opcion))
            return;

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

        if (FiltroFechaActivo)
        {
            filtradas = filtradas.Where(a => a.Fecha.Date >= FechaDesde.Date && a.Fecha.Date <= FechaHasta.Date);
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


    private FileResult? _archivoSeleccionado; // guardamos el FileResult completo, no solo la ruta

    [ObservableProperty]
    public partial string? NombreArchivoSeleccionado { get; set; }

    [ObservableProperty]
    public new partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? MensajeResultado { get; set; }

    [ObservableProperty]
    public partial bool HuboErrores { get; set; }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(PuedeSubirArchivo));
        OnPropertyChanged(nameof(TextoBotonSubir));
    }

    public bool PuedeSubirArchivo => !IsBusy;
    public string TextoBotonSubir => IsBusy ? "Subiendo..." : "Subir";

    partial void OnNombreArchivoSeleccionadoChanged(string? value)
    {
        OnPropertyChanged(nameof(TieneArchivoSeleccionado));
    }

    public bool TieneArchivoSeleccionado => !string.IsNullOrEmpty(NombreArchivoSeleccionado);

    partial void OnMensajeResultadoChanged(string? value)
    {
        OnPropertyChanged(nameof(TieneMensajeResultado));
    }

    public bool TieneMensajeResultado => !string.IsNullOrEmpty(MensajeResultado);


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

            var resultado = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona un archivo Excel",
                FileTypes = customFileType
            });

            if (resultado is null)
                return; // el usuario canceló, no es un error

            _archivoSeleccionado = resultado;
            NombreArchivoSeleccionado = resultado.FileName;
            MensajeResultado = null; // limpia el resultado de una carga previa
            HuboErrores = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al seleccionar archivo: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CargarArchivoAsync()
    {
        if (_archivoSeleccionado is null)
        {
            MensajeResultado = "Primero selecciona un archivo.";
            HuboErrores = true;
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            MensajeResultado = null;
            HuboErrores = false;

            await using var stream = await _archivoSeleccionado.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            var resultado = await _apiService.PostFileWithDetailsAsync<CargaActividadesResponseDto>(
                "carga-actividades/excel", // sin "api/" — el BaseUrl ya lo incluye
                memoryStream.ToArray(),
                _archivoSeleccionado.FileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            if (resultado is null)
            {
                MensajeResultado = "No se pudo interpretar la respuesta del servidor.";
                HuboErrores = true;
                return;
            }

            HuboErrores = !resultado.IsSuccess || resultado.ErroresValidacion.Count > 0;
            MensajeResultado = resultado.IsSuccess
                ? $"{resultado.Message} ({resultado.RegistrosProcesados} registros procesados)"
                : resultado.Message;

            if (resultado.IsSuccess)
            {
                // Refrescamos toda la lista desde el backend en vez de mapear
                // resultado.Actividades directo, porque ActividadCargadaDto no trae Id
                // y nuestro sistema de selección (_idsSeleccionados) depende de tenerlo.
                await CargarActividadesAsync();

                // Limpiamos la selección de archivo para que el usuario pueda cargar
                // otro sin ver el nombre del archivo anterior pegado en pantalla.
                _archivoSeleccionado = null;
                NombreArchivoSeleccionado = null;
            }
        }
        catch (Exception ex)
        {
            MensajeResultado = $"Error al subir el archivo: {ex.Message}";
            HuboErrores = true;
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    private void QuitarArchivoSeleccionado()
    {
        _archivoSeleccionado = null;
        NombreArchivoSeleccionado = null;
        MensajeResultado = null;
        HuboErrores = false;
    }


    [RelayCommand]
    [Obsolete]
    private async Task DescargarAsync()
    {
        ErrorMessage = string.Empty;
        DescargandoArchivo = true;
        try
        {
            if (_filtradasCache == null || _filtradasCache.Count == 0)
            {
                ErrorMessage = "No hay actividades para exportar con los filtros aplicados.";
                await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
                return;
            }

            var encabezados = new[]
            {
                "Fecha", "Colaborador", "Proyecto", "Cliente", "Líder técnico", "Horas", "Estado"
            };
            var filas = _filtradasCache.Select(a => new[]
            {
                a.Fecha.ToString("dd/MM/yyyy"),
                a.Colaborador,
                a.Proyecto,
                a.Cliente,
                a.LiderTecnico,
                a.NroHoras.ToString(CulturaEc),
                a.Estado
            }).ToList();

            await ReportService.SeleccionarYExportarAsync(
                "Reporte de Actividades", encabezados, filas, "Actividades");
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

}
