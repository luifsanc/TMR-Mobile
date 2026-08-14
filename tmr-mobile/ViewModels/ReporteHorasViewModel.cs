using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Reportes;

namespace tmr_mobile.ViewModels;

public partial class ReporteHorasViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private int _paginaActual = 1;
    private const int TamanoPagina = 20;
    private int _totalItems = 0;

    [ObservableProperty]
    public partial ObservableCollection<ReporteHorasResponse> Resultados { get; set; } = new();

    [ObservableProperty]
    public partial string TextoCliente { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int? MesSeleccionado { get; set; }

    [ObservableProperty]
    public partial int? AnioSeleccionado { get; set; } = null;

    [ObservableProperty]
    public partial ObservableCollection<string> AniosDisponibles { get; set; } = new() { "Todos", "2026", "2025", "2024" };

    [ObservableProperty]
    public partial ObservableCollection<string> MesesDisponibles { get; set; } = new() 
    { 
        "Todos", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", 
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" 
    };

    [ObservableProperty]
    public partial string AnioSeleccionadoTexto { get; set; } = "Todos";

    [ObservableProperty]
    public partial string MesSeleccionadoTexto { get; set; } = "Todos";

    [ObservableProperty]
    public partial decimal TotalHorasRegistradas { get; set; }

    [ObservableProperty]
    public partial int TotalClientesConsultados { get; set; }

    [ObservableProperty]
    public partial int TotalRecursosInvolucrados { get; set; }

    [ObservableProperty]
    private bool _estaCargandoMas;

    public ReporteHorasViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Reporte por Horas";
    }

    partial void OnTextoClienteChanged(string value)
    {
        _ = CargarReporteAsync();
    }

    partial void OnAnioSeleccionadoTextoChanged(string value)
    {
        if (int.TryParse(value, out int anio))
            AnioSeleccionado = anio;
        else
            AnioSeleccionado = null;

        _ = CargarReporteAsync();
    }

    partial void OnMesSeleccionadoTextoChanged(string value)
    {
        var index = MesesDisponibles.IndexOf(value);
        if (index > 0)
            MesSeleccionado = index;
        else
            MesSeleccionado = null;

        _ = CargarReporteAsync();
    }

    [RelayCommand]
    public async Task CargarReporteAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        _paginaActual = 1;

        try
        {
            var url = BuildUrl(_paginaActual, TamanoPagina);
            var respuesta = await _apiService.GetAsync<PaginatedResponse<ReporteHorasResponse>>(url);

            Resultados.Clear();
            if (respuesta?.Data != null)
            {
                _totalItems = respuesta.Total;
                foreach (var item in respuesta.Data)
                {
                    Resultados.Add(item);
                }

                if (respuesta.AnioMinimo.HasValue && respuesta.AnioMaximo.HasValue && respuesta.AnioMaximo.Value >= respuesta.AnioMinimo.Value)
                {
                    var listaAnios = new List<string> { "Todos" };
                    for (int y = respuesta.AnioMaximo.Value; y >= respuesta.AnioMinimo.Value; y--)
                    {
                        listaAnios.Add(y.ToString());
                    }
                    if (AniosDisponibles.Count != listaAnios.Count)
                    {
                        AniosDisponibles = new ObservableCollection<string>(listaAnios);
                    }
                }

                RecalcularMetricas();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar reporte por horas: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CargarMasReportesAsync()
    {
        if (EstaCargandoMas || IsBusy) return;
        if (Resultados.Count >= _totalItems && _totalItems > 0) return;

        EstaCargandoMas = true;
        _paginaActual++;

        try
        {
            var url = BuildUrl(_paginaActual, TamanoPagina);
            var respuesta = await _apiService.GetAsync<PaginatedResponse<ReporteHorasResponse>>(url);

            if (respuesta?.Data != null)
            {
                foreach (var item in respuesta.Data)
                {
                    Resultados.Add(item);
                }

                RecalcularMetricas();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar más reportes por horas: {ex.Message}");
        }
        finally
        {
            EstaCargandoMas = false;
        }
    }

    private string BuildUrl(int page, int pageSize)
    {
        var url = $"reportes/horas?Page={page}&PageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(TextoCliente))
            url += $"&Cliente={Uri.EscapeDataString(TextoCliente.Trim())}";
        if (MesSeleccionado.HasValue && MesSeleccionado.Value > 0)
            url += $"&Mes={MesSeleccionado.Value}";
        if (AnioSeleccionado.HasValue)
            url += $"&Anio={AnioSeleccionado.Value}";

        return url;
    }

    private void RecalcularMetricas()
    {
        TotalHorasRegistradas = Resultados.Sum(r => r.Horas);
        TotalClientesConsultados = Resultados.Select(r => r.Cliente).Distinct().Count();
        TotalRecursosInvolucrados = Resultados.Sum(r => r.Recursos);
    }

    [RelayCommand]
    private async Task ExportarReporteAsync()
    {
        var encabezados = new[] { "Cliente", "Recursos", "Horas", "Mes", "Año", "Estado Cliente" };
        var filas = Resultados.Select(r => new[]
        {
            r.Cliente,
            r.Recursos.ToString(),
            r.Horas.ToString("0.00"),
            r.Mes,
            r.Anio,
            r.EstadoCliente
        }).ToList();

        await ReportService.SeleccionarYExportarAsync(
            "Reporte por Horas Ejecutadas", encabezados, filas, "Reporte_Horas");
    }

    [RelayCommand]
    private async Task RegresarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
