using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_mobile.Views.Operaciones;
using tmr_shared.DTOs.Lideres;

namespace tmr_mobile.ViewModels;

public class ProyectoMinimalLiderDto
{
    public int? IdLider { get; set; }
}

public class ProyectoMinimalResponse
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int? IdLider { get; set; }
    public List<ProyectoMinimalLiderDto>? Lideres { get; set; }
}

public partial class LideresViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private List<LiderResponse> _todosLideres = new();
    private List<LiderResponse> _lideresFiltradosCache = new();
    
    private const int TamanoPagina = 15;
    private int _paginaActual = 1;

    public ObservableCollection<LiderResponse> Lideres { get; } = new();

    [ObservableProperty]
    private int _totalInternos;

    [ObservableProperty]
    private int _totalExternos;

    [ObservableProperty]
    private int _totalActivos;

    [ObservableProperty]
    private int _totalInactivos;

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    [ObservableProperty]
    private string _filtroEstado = "Todos";

    [ObservableProperty]
    private bool _estaCargandoMas;

    public LideresViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Gestión de Líderes";
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        FiltrarLideres(reset: true);
    }

    partial void OnFiltroEstadoChanged(string value)
    {
        FiltrarLideres(reset: true);
    }

    [RelayCommand]
    private async Task CargarLideresAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // 1. Cargar lista completa de líderes desde el Backend
            var respuestaLideres = await _apiService.GetAsync<List<LiderResponse>>("api/lideres");
            
            // 2. Cargar lista de proyectos para realizar el cruce estricto por líder y estado funcional activo (igual a la Web)
            var respuestaProyectos = await _apiService.GetAsync<List<ProyectoMinimalResponse>>("api/proyectos");

            if (respuestaLideres != null)
            {
                if (respuestaProyectos != null)
                {
                    foreach (var lider in respuestaLideres)
                    {
                        // Estricto a la Web: Filtrar proyectos asignados como Líder Principal con estado funcional Activo / En progreso
                        var proyectosDelLider = respuestaProyectos
                            .Where(p => p.IdLider.HasValue && p.IdLider.Value == lider.Id &&
                                       (!string.IsNullOrEmpty(p.Estado) && (p.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase) || p.Estado.Equals("En progreso", StringComparison.OrdinalIgnoreCase))))
                            .Select(p => new ProyectoAsignadoDTO
                            {
                                Id = p.Id,
                                Codigo = p.Codigo,
                                Nombre = p.Nombre,
                                Cliente = p.Cliente,
                                Estado = p.Estado
                            })
                            .ToList();

                        lider.Proyectos = proyectosDelLider;
                    }
                }

                _todosLideres = respuestaLideres;
                FiltrarLideres(reset: true);
            }

            // 3. Cargar contadores de métricas desde el Backend
            var contadores = await _apiService.GetAsync<ContadoresLiderResponse>("api/lideres/contadores");
            if (contadores != null)
            {
                TotalInternos = contadores.Internos;
                TotalExternos = contadores.Externos;
                TotalActivos = contadores.Activos;
                TotalInactivos = contadores.Inactivos;
            }
            else
            {
                TotalInternos = _todosLideres.Count(l => l.TipoBadge == "Interno");
                TotalExternos = _todosLideres.Count(l => l.TipoBadge == "Externo");
                TotalActivos = _todosLideres.Count(l => l.Activo);
                TotalInactivos = _todosLideres.Count(l => !l.Activo);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar líderes: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FiltrarLideres(bool reset = true)
    {
        if (reset)
        {
            _paginaActual = 1;
            Lideres.Clear();

            var filtrados = _todosLideres.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                var query = TextoBusqueda.ToLowerInvariant().Trim();
                filtrados = filtrados.Where(l => 
                    l.NombreCompleto.ToLowerInvariant().Contains(query) ||
                    (l.Email != null && l.Email.ToLowerInvariant().Contains(query)) ||
                    (l.TipoBadge != null && l.TipoBadge.ToLowerInvariant().Contains(query)) ||
                    (l.ClientesResumen != null && l.ClientesResumen.ToLowerInvariant().Contains(query)));
            }

            if (FiltroEstado == "Activos")
            {
                filtrados = filtrados.Where(l => l.Activo);
            }
            else if (FiltroEstado == "Inactivos")
            {
                filtrados = filtrados.Where(l => !l.Activo);
            }

            _lideresFiltradosCache = filtrados.ToList();
        }

        var lote = _lideresFiltradosCache
            .Skip((_paginaActual - 1) * TamanoPagina)
            .Take(TamanoPagina);

        foreach (var lider in lote)
        {
            Lideres.Add(lider);
        }
    }

    [RelayCommand]
    private void CargarMasLideres()
    {
        if (EstaCargandoMas || IsBusy) return;

        var totalItemsCargados = Lideres.Count;
        if (totalItemsCargados >= _lideresFiltradosCache.Count) return;

        EstaCargandoMas = true;
        _paginaActual++;

        FiltrarLideres(reset: false);

        EstaCargandoMas = false;
    }

    [RelayCommand]
    private async Task ExportarReporteAsync()
    {
        if (Lideres.Count == 0)
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

        if (opcion == "📄 Descargar PDF")
        {
            var encabezados = new[] { "Nombre", "Correo", "Teléfono", "Tipo", "Clientes Vinculados", "Estado" };
            var filas = _lideresFiltradosCache.Select(l => new[]
            {
                l.NombreCompleto,
                l.Email ?? "-",
                l.Telefono ?? "-",
                l.TipoBadge,
                l.ClientesResumen,
                l.EstadoTexto
            }).ToList();

            await ReportService.ExportarHtmlPdfAsync("Reporte de Líderes", encabezados, filas, "Lideres");
        }
        else if (opcion == "📊 Descargar Excel")
        {
            var encabezados = new[] { "Nombre", "Correo", "Teléfono", "Tipo", "Clientes Vinculados", "Estado" };
            var filas = _lideresFiltradosCache.Select(l => new[]
            {
                l.NombreCompleto,
                l.Email ?? "-",
                l.Telefono ?? "-",
                l.TipoBadge,
                l.ClientesResumen,
                l.EstadoTexto
            }).ToList();

            await ReportService.ExportarCsvAsync("Reporte de Líderes", encabezados, filas, "Lideres");
        }
    }

    [RelayCommand]
    private async Task AbrirOpcionesAsync(LiderResponse lider)
    {
        if (lider == null) return;

        var estadoAccion = lider.Activo ? "Desactivar" : "Activar";
        var opcion = await Shell.Current.DisplayActionSheet(
            title: $"{lider.NombreCompleto}",
            cancel: "Cancelar",
            destruction: "Eliminar",
            buttons: new[] { "👁️ Ver más", "✏️ Editar", $"🔄 {estadoAccion}" }
        );

        if (opcion == "👁️ Ver más")
        {
            await VerDetalleAsync(lider);
        }
        else if (opcion == "✏️ Editar")
        {
            await EditarLiderAsync(lider);
        }
        else if (opcion == $"🔄 {estadoAccion}")
        {
            await ToggleEstadoLiderAsync(lider);
        }
        else if (opcion == "Eliminar")
        {
            await EliminarLiderAsync(lider);
        }
    }

    [RelayCommand]
    private async Task VerDetalleAsync(LiderResponse lider)
    {
        if (lider == null) return;
        await Shell.Current.Navigation.PushModalAsync(new LiderDetailPage(lider));
    }

    [RelayCommand]
    private async Task AbrirCrearLiderAsync()
    {
        var page = new LiderFormPage(_apiService);
        await Shell.Current.Navigation.PushModalAsync(page);
        await CargarLideresAsync();
    }

    [RelayCommand]
    private async Task EditarLiderAsync(LiderResponse lider)
    {
        if (lider == null) return;
        var page = new LiderFormPage(_apiService, lider);
        await Shell.Current.Navigation.PushModalAsync(page);
        await CargarLideresAsync();
    }

    [RelayCommand]
    private async Task ToggleEstadoLiderAsync(LiderResponse lider)
    {
        if (lider == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Confirmación",
            $"¿Estás seguro de que deseas {(lider.Activo ? "desactivar" : "activar")} a {lider.NombreCompleto}?",
            "Sí", "No"
        );

        if (confirm)
        {
            var updateReq = new ActualizarLiderRequest
            {
                Nombres = lider.Nombres,
                Apellidos = lider.Apellidos,
                Email = lider.Email,
                Telefono = lider.Telefono,
                Tipopersona = lider.Tipopersona,
                Idtipo = lider.Idtipo,
                NumeroIdentificacion = lider.NumeroIdentificacion,
                Activo = !lider.Activo
            };

            var ok = await _apiService.PutAsync($"api/lideres/{lider.Id}", updateReq);
            if (ok)
            {
                await CargarLideresAsync();
            }
        }
    }

    [RelayCommand]
    private async Task EliminarLiderAsync(LiderResponse lider)
    {
        if (lider == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Eliminar Líder",
            $"¿Estás seguro de eliminar a {lider.NombreCompleto}?",
            "Eliminar", "Cancelar"
        );

        if (confirm)
        {
            var ok = await _apiService.DeleteAsync($"api/lideres/{lider.Id}");
            if (ok)
            {
                await CargarLideresAsync();
            }
        }
    }
}
