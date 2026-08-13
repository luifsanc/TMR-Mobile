using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_shared.DTOs.Reportes;

namespace tmr_mobile.ViewModels;

public partial class ReporteFechasViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private int _paginaActual = 1;
    private const int TamanoPagina = 20;
    private int _totalItems = 0;

    [ObservableProperty]
    public partial ObservableCollection<ReporteFechasResponse> Resultados { get; set; } = new();

    [ObservableProperty]
    public partial DateTime? FechaInicio { get; set; } = null;

    [ObservableProperty]
    public partial DateTime? FechaFin { get; set; } = null;

    [ObservableProperty]
    public partial string TextoBusqueda { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TextoCliente { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TextoLider { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int TotalAsignaciones { get; set; }

    [ObservableProperty]
    public partial int TotalRecursosUnicos { get; set; }

    [ObservableProperty]
    public partial int TotalLideresUnicos { get; set; }

    [ObservableProperty]
    private bool _estaCargandoMas;

    public ReporteFechasViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Reporte por Fechas";
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        _ = CargarReporteAsync();
    }

    partial void OnTextoClienteChanged(string value)
    {
        _ = CargarReporteAsync();
    }

    partial void OnTextoLiderChanged(string value)
    {
        _ = CargarReporteAsync();
    }

    partial void OnFechaInicioChanged(DateTime? value)
    {
        _ = CargarReporteAsync();
    }

    partial void OnFechaFinChanged(DateTime? value)
    {
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
            var respuesta = await _apiService.GetAsync<PaginatedResponse<ReporteFechasResponse>>(url);

            Resultados.Clear();
            if (respuesta?.Data != null)
            {
                _totalItems = respuesta.Total;
                foreach (var item in respuesta.Data)
                {
                    Resultados.Add(item);
                }

                RecalcularMetricas();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar reporte por fechas: {ex.Message}";
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
            var respuesta = await _apiService.GetAsync<PaginatedResponse<ReporteFechasResponse>>(url);

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
            System.Diagnostics.Debug.WriteLine($"Error al cargar más reportes por fechas: {ex.Message}");
        }
        finally
        {
            EstaCargandoMas = false;
        }
    }

    private string BuildUrl(int page, int pageSize)
    {
        var url = $"reportes/fechas?Page={page}&PageSize={pageSize}";
        if (FechaInicio.HasValue)
            url += $"&FechaInicio={FechaInicio.Value:yyyy-MM-dd}";
        if (FechaFin.HasValue)
            url += $"&FechaFin={FechaFin.Value:yyyy-MM-dd}";

        var term = !string.IsNullOrWhiteSpace(TextoBusqueda)
            ? TextoBusqueda.Trim()
            : (!string.IsNullOrWhiteSpace(TextoCliente) ? TextoCliente.Trim() : string.Empty);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var escaped = Uri.EscapeDataString(term);
            url += $"&Cliente={escaped}&Lider={escaped}";
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(TextoLider))
                url += $"&Lider={Uri.EscapeDataString(TextoLider.Trim())}";
        }

        return url;
    }

    private void RecalcularMetricas()
    {
        TotalAsignaciones = _totalItems > 0 ? _totalItems : Resultados.Count;
        TotalRecursosUnicos = Resultados.Select(r => r.Recurso).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().Count();
        TotalLideresUnicos = Resultados.Select(r => r.Lider).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().Count();
    }

    [RelayCommand]
    private async Task ExportarReporteAsync()
    {
        if (Resultados.Count == 0)
        {
            await Shell.Current.DisplayAlert("Exportar", "No hay datos para exportar.", "OK");
            return;
        }

        var opcion = await Shell.Current.DisplayActionSheet(
            title: "Seleccione el formato de descarga:",
            cancel: "Cancelar",
            destruction: null,
            buttons: new[] { "📄 Descargar PDF", "📊 Descargar Excel" }
        );

        var encabezados = new[] { "Cliente", "Líder", "Recurso", "Cargo", "Fecha Inicio", "Fecha Fin" };
        var filas = Resultados.Select(r => new[]
        {
            r.Cliente ?? "-",
            r.Lider ?? "-",
            r.Recurso ?? "-",
            r.Cargo ?? "-",
            r.FechaInicioFormateada,
            r.FechaFinFormateada
        }).ToList();

        if (opcion == "📄 Descargar PDF")
        {
            await ReportService.ExportarHtmlPdfAsync("Reporte por Rango de Fechas", encabezados, filas, "Reporte_Fechas");
        }
        else if (opcion == "📊 Descargar Excel")
        {
            await ReportService.ExportarCsvAsync("Reporte por Rango de Fechas", encabezados, filas, "Reporte_Fechas");
        }
    }

    [RelayCommand]
    private async Task RegresarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
