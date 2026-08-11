using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile.ViewModels;

public partial class CatalogosConfigViewModel : BaseViewModel
{
    private readonly ICatalogosService _catalogosService;

    private readonly List<CatalogoDetalle> _todosDetalles = new();

    private const int PageSize = 5;


    // ─────────────────────────────────────────────
    // COLECCIONES
    // ─────────────────────────────────────────────

    public ObservableCollection<CatalogoMaster> Catalogos
    {
        get;
    } = new();

    public ObservableCollection<CatalogoDetalle> Detalles
    {
        get;
    } = new();

    public ObservableCollection<int> PaginasVisibles
    {
        get;
    } = new();


    // ─────────────────────────────────────────────
    // CATÁLOGO SELECCIONADO
    // ─────────────────────────────────────────────

    [ObservableProperty]
    public partial CatalogoMaster? CatalogoSeleccionado
    {
        get;
        set;
    }


    // ─────────────────────────────────────────────
    // BÚSQUEDA Y FILTROS
    // ─────────────────────────────────────────────

    [ObservableProperty]
    public partial string Busqueda
    {
        get;
        set;
    } = string.Empty;

    [ObservableProperty]
    public partial string FiltroEstado
    {
        get;
        set;
    } = "Todos";


    // ─────────────────────────────────────────────
    // PAGINACIÓN
    // ─────────────────────────────────────────────

    [ObservableProperty]
    public partial int PaginaActual
    {
        get;
        set;
    } = 1;

    [ObservableProperty]
    public partial int TotalPaginas
    {
        get;
        set;
    } = 1;

    [ObservableProperty]
    public partial int TotalRegistros
    {
        get;
        set;
    }


    // ─────────────────────────────────────────────
    // RESUMEN
    // ─────────────────────────────────────────────

    [ObservableProperty]
    public partial int TotalActivos
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial int TotalInactivos
    {
        get;
        set;
    }

    public int TotalItems =>
        _todosDetalles.Count;

    public bool PuedeAnterior =>
        PaginaActual > 1;

    public bool PuedeSiguiente =>
        PaginaActual < TotalPaginas;


    // ─────────────────────────────────────────────
    // CONSTRUCTOR
    // ─────────────────────────────────────────────

    public CatalogosConfigViewModel(
        ICatalogosService catalogosService)
    {
        _catalogosService = catalogosService;

        Title = "Catálogos";
    }


    // ─────────────────────────────────────────────
    // INICIALIZACIÓN / REFRESCO
    // ─────────────────────────────────────────────

    public async Task InicializarAsync()
    {
        // Primera vez que entramos
        if (Catalogos.Count == 0)
        {
            await CargarCatalogosAsync();
            return;
        }

        // Si volvemos de crear o editar,
        // recargamos el catálogo seleccionado
        if (CatalogoSeleccionado is not null)
        {
            await CargarDetallesAsync(
                CatalogoSeleccionado.Id
            );
        }
    }


    private async Task CargarCatalogosAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var catalogos =
                await _catalogosService
                    .ObtenerCatalogosAsync();

            Catalogos.Clear();

            foreach (var catalogo in catalogos)
            {
                if (catalogo.Activo)
                {
                    Catalogos.Add(catalogo);
                }
            }

            if (Catalogos.Count > 0)
            {
                await SeleccionarCatalogoAsync(
                    Catalogos[0]
                );
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "No se pudieron cargar los catálogos.";

            System.Diagnostics.Debug.WriteLine(
                ex.Message
            );
        }
        finally
        {
            IsBusy = false;
        }
    }


    // ─────────────────────────────────────────────
    // SELECCIONAR CATÁLOGO
    // ─────────────────────────────────────────────

    [RelayCommand]
    private async Task SeleccionarCatalogoAsync(
        CatalogoMaster catalogo)
    {
        if (catalogo is null)
            return;

        foreach (var item in Catalogos)
        {
            item.IsSelected =
                item.Id == catalogo.Id;
        }

        CatalogoSeleccionado =
            catalogo;

        Busqueda =
            string.Empty;

        FiltroEstado =
            "Todos";

        PaginaActual =
            1;

        await CargarDetallesAsync(
            catalogo.Id
        );
    }


    // ─────────────────────────────────────────────
    // NUEVO
    // ─────────────────────────────────────────────

    [RelayCommand]
    private async Task NuevoAsync()
    {
        if (CatalogoSeleccionado is null)
            return;

        var parametros =
            new Dictionary<string, object>
            {
                ["Catalogo"] =
                    CatalogoSeleccionado
            };

        await Shell.Current.GoToAsync(
            nameof(CatalogoFormPage),
            parametros
        );
    }


    // ─────────────────────────────────────────────
    // CARGAR DETALLES
    // ─────────────────────────────────────────────

    private async Task CargarDetallesAsync(
        int idCatalogo)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var detalles =
                await _catalogosService
                    .ObtenerDetallesAsync(
                        idCatalogo
                    );

            _todosDetalles.Clear();

            _todosDetalles.AddRange(
                detalles
            );

            AplicarFiltros();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "No se pudieron cargar los registros.";

            System.Diagnostics.Debug.WriteLine(
                ex.Message
            );
        }
        finally
        {
            IsBusy = false;
        }
    }


    // ─────────────────────────────────────────────
    // EVENTOS DE FILTROS
    // ─────────────────────────────────────────────

    partial void OnBusquedaChanged(
        string value)
    {
        PaginaActual = 1;

        AplicarFiltros();
    }

    partial void OnFiltroEstadoChanged(
        string value)
    {
        PaginaActual = 1;

        AplicarFiltros();
    }


    // ─────────────────────────────────────────────
    // FILTRADO
    // ─────────────────────────────────────────────

    private void AplicarFiltros()
    {
        // Resumen general del catálogo
        TotalActivos =
            _todosDetalles.Count(
                x => x.Activo
            );

        TotalInactivos =
            _todosDetalles.Count(
                x => !x.Activo
            );

        OnPropertyChanged(
            nameof(TotalItems)
        );


        IEnumerable<CatalogoDetalle> resultado =
            _todosDetalles;


        // Estado
        if (FiltroEstado == "Activos")
        {
            resultado =
                resultado.Where(
                    x => x.Activo
                );
        }
        else if (
            FiltroEstado == "Inactivos")
        {
            resultado =
                resultado.Where(
                    x => !x.Activo
                );
        }


        // Buscador
        if (
            !string.IsNullOrWhiteSpace(
                Busqueda
            )
        )
        {
            var texto =
                Busqueda
                    .Trim()
                    .ToLowerInvariant();

            resultado =
                resultado.Where(
                    x =>
                        x.CodigoValor
                            .ToLowerInvariant()
                            .Contains(texto)
                        ||
                        x.Valor
                            .ToLowerInvariant()
                            .Contains(texto)
                        ||
                        (x.Descripcion ?? string.Empty)
                            .ToLowerInvariant()
                            .Contains(texto)
                );
        }


        // Orden
        resultado =
            resultado
                .OrderBy(
                    x =>
                        x.Orden.HasValue
                            ? x.Orden.Value
                            : int.MaxValue
                )
                .ThenBy(
                    x => x.Valor
                );


        var lista =
            resultado.ToList();


        // ─────────────────────────────────────────
        // PAGINACIÓN
        // ─────────────────────────────────────────

        TotalRegistros =
            lista.Count;

        TotalPaginas =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    TotalRegistros /
                    (double)PageSize
                )
            );

        if (
            PaginaActual > TotalPaginas)
        {
            PaginaActual =
                TotalPaginas;
        }


        var pagina =
            lista
                .Skip(
                    (PaginaActual - 1)
                    * PageSize
                )
                .Take(PageSize)
                .ToList();


        Detalles.Clear();

        foreach (
            var detalle in pagina)
        {
            Detalles.Add(
                detalle
            );
        }


        ActualizarPaginas();


        OnPropertyChanged(
            nameof(PuedeAnterior)
        );

        OnPropertyChanged(
            nameof(PuedeSiguiente)
        );
    }


    // ─────────────────────────────────────────────
    // NÚMEROS DE PÁGINA
    // ─────────────────────────────────────────────

    private void ActualizarPaginas()
    {
        PaginasVisibles.Clear();

        var inicio =
            Math.Max(
                1,
                PaginaActual - 2
            );

        var fin =
            Math.Min(
                TotalPaginas,
                inicio + 4
            );

        if (
            fin - inicio < 4)
        {
            inicio =
                Math.Max(
                    1,
                    fin - 4
                );
        }

        for (
            var pagina = inicio;
            pagina <= fin;
            pagina++)
        {
            PaginasVisibles.Add(
                pagina
            );
        }
    }


    // ─────────────────────────────────────────────
    // FILTROS
    // ─────────────────────────────────────────────

    [RelayCommand]
    private void MostrarTodos()
    {
        FiltroEstado =
            "Todos";
    }

    [RelayCommand]
    private void MostrarActivos()
    {
        FiltroEstado =
            "Activos";
    }

    [RelayCommand]
    private void MostrarInactivos()
    {
        FiltroEstado =
            "Inactivos";
    }


    // ─────────────────────────────────────────────
    // PAGINACIÓN
    // ─────────────────────────────────────────────

    [RelayCommand]
    private void PaginaAnterior()
    {
        if (PaginaActual <= 1)
            return;

        PaginaActual--;

        AplicarFiltros();
    }

    [RelayCommand]
    private void PaginaSiguiente()
    {
        if (
            PaginaActual >= TotalPaginas)
        {
            return;
        }

        PaginaActual++;

        AplicarFiltros();
    }

    [RelayCommand]
    private void IrPagina(
        int pagina)
    {
        if (
            pagina < 1 ||
            pagina > TotalPaginas)
        {
            return;
        }

        PaginaActual =
            pagina;

        AplicarFiltros();
    }


    // ─────────────────────────────────────────────
    // ABRIR DETALLE
    // ─────────────────────────────────────────────

    [RelayCommand]
    private async Task AbrirDetalleAsync(
        CatalogoDetalle detalle)
    {
        if (detalle is null)
            return;

        var parametros =
            new Dictionary<string, object>
            {
                ["Detalle"] =
                    detalle
            };

        if (
            CatalogoSeleccionado
            is not null)
        {
            parametros["Catalogo"] =
                CatalogoSeleccionado;
        }

        await Shell.Current.GoToAsync(
            nameof(CatalogoDetallePage),
            parametros
        );
    }
}