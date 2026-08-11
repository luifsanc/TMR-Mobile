using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models;
using tmr_mobile.Services;
using tmr_shared.DTOs.CargaActividades;
using tmr_shared.DTOs.Dashboard;
using System.Globalization;

namespace tmr_mobile.ViewModels;

public partial class CargaActividadesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    // Cache completo sin filtrar. Se descarga una vez y el filtro/búsqueda
    // se resuelve en memoria, para no golpear la API en cada tecla.
    private List<ActividadCargaResponse> _todasLasActividades = new();

    private const int TamañoPagina = 10;
    private List<ActividadCargaResponse> _filtradasCache = new();

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

    public bool TieneSeleccionActividades => Actividades.Any(a => a.IsSeleccionado);

    partial void OnTieneHorasFaltantesChanged(bool value)
    {
        OnPropertyChanged(nameof(NoTieneHorasFaltantes));
    }

    [ObservableProperty]
    public partial bool CargandoActividades { get; set; }

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
        Actividades.Clear();

        var inicio = (PaginaActual - 1) * TamañoPagina;

        foreach (var item in _filtradasCache.Skip(inicio).Take(TamañoPagina))
        {
            Actividades.Add(new ActividadCargaItem
            {
                Colaborador = item.Colaborador,
                Proyecto = item.Proyecto,
                Cliente = item.Cliente,
                LiderTecnico = item.LiderTecnico,
                FechaTexto = item.Fecha.ToString("dd/MM/yyyy"),
                HorasTexto = $"{item.NroHoras:0.##} h",
                Estado = item.Estado
            });
        }

        RangoRegistrosTexto = _filtradasCache.Count == 0
            ? "Sin resultados"
            : $"{inicio + 1} al {Math.Min(PaginaActual * TamañoPagina, _filtradasCache.Count)} de {_filtradasCache.Count}";

        OnPropertyChanged(nameof(PuedeIrAnterior));
        OnPropertyChanged(nameof(PuedeIrSiguiente));
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
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
    private async Task Descargar()
    {
        var seleccionadas = Actividades.Where(a => a.IsSeleccionado).ToList();
        // TODO: exportar 'seleccionadas' a Excel. Pendiente por ahora.
    }
    /*
        [RelayCommand]
        private async Task SeleccionarArchivoAsync()
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "com.microsoft.excel.xls", "org.openxmlformats.spreadsheetml.sheet" } },
                    { DevicePlatform.Android, new[] { "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                    { DevicePlatform.WinUI, new[] { ".xlsx", ".xls" } },
                });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Seleccionar Excel de Actividades",
                FileTypes = customFileType
            });

            if (result != null)
            {
                _archivoElegido = result;
                ArchivoSeleccionado = result.FileName;
                TieneArchivoSeleccionado = true;
                ResultadoMensaje = string.Empty;
            }
        }

        [RelayCommand]
        private async Task ProcesarArchivoAsync()
        {
            if (_archivoElegido == null) return;

            IsBusy = true;
            ResultadoMensaje = string.Empty;

            try
            {
                using var stream = await _archivoElegido.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var resultado = await _cargaActividadesService.SubirExcelAsync(fileBytes, _archivoElegido.FileName);

                if (resultado.IsSuccess)
                {
                    ResultadoMensaje = $"{resultado.Message} ({resultado.RegistrosProcesados} registros procesados)";
                    ArchivoSeleccionado = "Ningún archivo seleccionado";
                    TieneArchivoSeleccionado = false;
                    _archivoElegido = null;
                    await CargarActividadesAsync();
                }
                else
                {
                    ResultadoMensaje = resultado.ErroresValidacion.Count > 0
                        ? $"{resultado.Message}: {string.Join(", ", resultado.ErroresValidacion)}"
                        : resultado.Message;
                }
            }
            catch (Exception ex)
            {
                ResultadoMensaje = $"Error al procesar el archivo: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }*/
}