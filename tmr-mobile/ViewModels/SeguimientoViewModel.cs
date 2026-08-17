using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using tmr_mobile.Models.Seguimiento;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public class SeguimientoViewModel : BaseViewModel
{
    private readonly ISeguimientoService _seguimientoService;
    private ObservableCollection<SeguimientoColaboradorDto> _colaboradores;
    private ObservableCollection<SeguimientoColaboradorDto> _colaboradoresFiltrados;
    private string _busqueda = string.Empty;
    private DateTime _fechaDesde = DateTime.Now.AddDays(-30);
    private DateTime _fechaHasta = DateTime.Now;

    public ObservableCollection<SeguimientoColaboradorDto> ColaboradoresFiltrados
    {
        get => _colaboradoresFiltrados;
        set { _colaboradoresFiltrados = value; OnPropertyChanged(); }
    }

    public string Busqueda
    {
        get => _busqueda;
        set 
        { 
            _busqueda = value; 
            OnPropertyChanged();
            FiltrarColaboradores();
        }
    }

    public DateTime FechaDesde
    {
        get => _fechaDesde;
        set 
        { 
            if (_fechaDesde != value)
            {
                _fechaDesde = value; 
                OnPropertyChanged();
                _ = CargarDatosAsync();
            }
        }
    }

    public DateTime FechaHasta
    {
        get => _fechaHasta;
        set 
        { 
            if (_fechaHasta != value)
            {
                _fechaHasta = value; 
                OnPropertyChanged();
                _ = CargarDatosAsync();
            }
        }
    }

    // Indicadores
    private decimal _horasPorRegistrar;
    public decimal HorasPorRegistrar { get => _horasPorRegistrar; set { _horasPorRegistrar = value; OnPropertyChanged(); } }

    private decimal _horasRegistradas;
    public decimal HorasRegistradas { get => _horasRegistradas; set { _horasRegistradas = value; OnPropertyChanged(); } }

    private decimal _promedioPorDia;
    public decimal PromedioPorDia { get => _promedioPorDia; set { _promedioPorDia = value; OnPropertyChanged(); } }

    private int _colaboradoresActivos;
    public int ColaboradoresActivos { get => _colaboradoresActivos; set { _colaboradoresActivos = value; OnPropertyChanged(); } }

    private int _colaboradoresConReporte;
    public int ColaboradoresConReporte { get => _colaboradoresConReporte; set { _colaboradoresConReporte = value; OnPropertyChanged(); } }

    private int _proyectosConActividades;
    public int ProyectosConActividades { get => _proyectosConActividades; set { _proyectosConActividades = value; OnPropertyChanged(); } }

    public ICommand BuscarCommand { get; }
    public ICommand AplicarFiltrosCommand { get; }
    public ICommand ItemTappedCommand { get; }
    public ICommand ExportarTodosCommand { get; }
    public ICommand DescargarSeleccionadosCommand { get; }

    public SeguimientoViewModel(ISeguimientoService seguimientoService)
    {
        _seguimientoService = seguimientoService;
        _colaboradores = new ObservableCollection<SeguimientoColaboradorDto>();
        _colaboradoresFiltrados = new ObservableCollection<SeguimientoColaboradorDto>();

        BuscarCommand = new Command(async () => await CargarDatosAsync());
        AplicarFiltrosCommand = new Command(async () => await CargarDatosAsync());
        ItemTappedCommand = new Command<SeguimientoColaboradorDto>(async (item) => await OnItemTapped(item));
        ExportarTodosCommand = new Command(async () => await ExportarTodosAsync());
        DescargarSeleccionadosCommand = new Command(async () => await DescargarSeleccionadosAsync());

        _ = CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            
            var filtro = new FiltroSeguimientoDto
            {
                Busqueda = string.IsNullOrWhiteSpace(Busqueda) ? null : Busqueda,
                FechaDesde = FechaDesde.ToString("yyyy-MM-dd"),
                FechaHasta = FechaHasta.ToString("yyyy-MM-dd")
            };

            var data = await _seguimientoService.ObtenerSeguimientoAsync(filtro);
            _colaboradores = new ObservableCollection<SeguimientoColaboradorDto>(data);
            
            CalcularIndicadores();
            FiltrarColaboradores();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No fue posible obtener la información.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CalcularIndicadores()
    {
        if (_colaboradores == null || !_colaboradores.Any())
        {
            HorasRegistradas = 0;
            HorasPorRegistrar = 0;
            PromedioPorDia = 0;
            ColaboradoresActivos = 0;
            ColaboradoresConReporte = 0;
            ProyectosConActividades = 0;
            return;
        }

        HorasRegistradas = _colaboradores.Sum(c => c.NroHoras);
        HorasPorRegistrar = _colaboradores.Sum(c => c.DiasACompletar * 8); // Estimación de 8 horas por día
        ColaboradoresActivos = _colaboradores.Count;
        ColaboradoresConReporte = _colaboradores.Count(c => c.DiasConReporte > 0);
        
        var totalDiasConReporte = _colaboradores.Sum(c => c.DiasConReporte);
        PromedioPorDia = totalDiasConReporte > 0 ? Math.Round(HorasRegistradas / totalDiasConReporte, 2) : 0;
        
        ProyectosConActividades = _colaboradores
            .SelectMany(c => c.Proyecto.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
            .Where(p => p != "Sin Proyecto")
            .Distinct()
            .Count();
    }

    private void FiltrarColaboradores()
    {
        if (string.IsNullOrWhiteSpace(Busqueda))
        {
            ColaboradoresFiltrados = new ObservableCollection<SeguimientoColaboradorDto>(_colaboradores);
        }
        else
        {
            var term = Busqueda.ToLower();
            var filtered = _colaboradores.Where(c => 
                c.Nombre.ToLower().Contains(term) || 
                c.Proyecto.ToLower().Contains(term) ||
                c.Cliente.ToLower().Contains(term)
            );
            ColaboradoresFiltrados = new ObservableCollection<SeguimientoColaboradorDto>(filtered);
        }
    }

    private async Task OnItemTapped(SeguimientoColaboradorDto item)
    {
        if (item == null) return;
        var navigationParameter = new Dictionary<string, object>
        {
            { "Colaborador", item }
        };
        await Shell.Current.GoToAsync(nameof(tmr_mobile.Views.Seguimiento.SeguimientoDetallePage), true, navigationParameter);
    }

    private async Task ExportarTodosAsync()
    {
        if (ColaboradoresFiltrados == null || !ColaboradoresFiltrados.Any())
        {
            await Application.Current.MainPage.DisplayAlert("Exportar", "No hay datos para exportar.", "OK");
            return;
        }
        await ExportarAExcelAsync(ColaboradoresFiltrados, "Consolidado_Seguimiento");
    }

    private async Task DescargarSeleccionadosAsync()
    {
        var seleccionados = ColaboradoresFiltrados?.Where(c => c.IsSelected).ToList();
        if (seleccionados == null || !seleccionados.Any())
        {
            await Application.Current.MainPage.DisplayAlert("Descargar", "Debe seleccionar al menos un colaborador.", "OK");
            return;
        }

        bool errores = false;
        int generados = 0;
        var svc = new ExcelExportService();

        foreach (var col in seleccionados)
        {
            try
            {
                var response = await _seguimientoService.ObtenerActividadesColaboradorAsync(
                    col.Id, 
                    FechaDesde.ToString("yyyy-MM-dd"), 
                    FechaHasta.ToString("yyyy-MM-dd"));

                if (response == null || !response.Actividades.Any())
                {
                    await Application.Current.MainPage.DisplayAlert("Aviso", $"No se encontraron actividades detalladas para {col.Nombre}.", "OK");
                    continue;
                }

                var bytes = svc.GenerarReporteDetalleColaborador(
                    col.Nombre,
                    FechaDesde,
                    FechaHasta,
                    response.Actividades,
                    response.Feriados ?? new List<string>()
                );

                var nombreArchivo = $"Reporte_{col.Nombre.Replace(" ", "_")}.xlsx";
                await tmr_mobile.Services.DescargaArchivoHelper.GuardarYCompartirAsync(
                    bytes,
                    nombreArchivo,
                    tmr_mobile.Services.DescargaArchivoHelper.MimeTypeXlsx,
                    $"Compartir Reporte de {col.Nombre}");
                    
                generados++;
            }
            catch (Exception ex)
            {
                errores = true;
                System.Diagnostics.Debug.WriteLine($"Error al descargar reporte para {col.Nombre}: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error con {col.Nombre}: {ex.Message}", "OK");
            }
        }

        if (errores)
        {
            await Application.Current.MainPage.DisplayAlert("Aviso", "Ocurrieron problemas al generar uno o más reportes.", "OK");
        }
        else if (generados > 0)
        {
            await Application.Current.MainPage.DisplayAlert("Éxito", $"Se generaron {generados} reporte(s) correctamente.", "OK");
        }
    }

    private async Task ExportarAExcelAsync(IEnumerable<SeguimientoColaboradorDto> datos, string nombreArchivo)
    {
        var encabezados = new[] { "Colaborador", "Proyecto", "Cliente", "Líder Técnico", "Horas Registradas", "Seguimiento", "Días con Reporte", "Días a Completar" };
        var filas = datos.Select(c => new[]
        {
            c.Nombre ?? "-",
            c.Proyecto ?? "-",
            c.Cliente ?? "-",
            c.LiderTecnico ?? "-",
            c.NroHoras.ToString(),
            c.Estado ?? "-",
            c.DiasConReporte.ToString(),
            c.DiasACompletar.ToString()
        }).ToList();

        await ReportService.SeleccionarYExportarAsync(
            $"Periodo: {FechaDesde:dd/MM/yyyy} al {FechaHasta:dd/MM/yyyy}", 
            encabezados, 
            filas, 
            nombreArchivo);
    }
}
