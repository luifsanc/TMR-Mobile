using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    private const int MaxResultadosVisibles = 200;

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

    partial void OnTextoBusquedaChanged(string value) => AplicarFiltro();
    partial void OnCampoFiltroChanged(string value) => AplicarFiltro();

    [RelayCommand]
    private async Task CargarActividadesAsync()
    {
        CargandoActividades = true;
        ErrorMessage = string.Empty;
        try
        {
            var response = await _apiService.GetAsync<List<ActividadCargaResponse>>("api/carga-actividades");
            _todasLasActividades = response ?? new List<ActividadCargaResponse>();
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

    private void AplicarFiltro()
    {
        Actividades.Clear();

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

        foreach (var item in filtradas.OrderByDescending(a => a.Fecha).Take(MaxResultadosVisibles))
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
    }

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
            ArchivoSeleccionado = result.FileName;
        }
    }
}
