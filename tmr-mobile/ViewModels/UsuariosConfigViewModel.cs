using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile.ViewModels;

public partial class UsuariosConfigViewModel : BaseViewModel
{
    private readonly IUsuariosService _usuariosService;
    private bool? _filtroActivo;

    public ObservableCollection<UsuarioListaItem> Usuarios { get; } = new();

    [ObservableProperty]
    public partial string Busqueda { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FiltroEstado { get; set; } = "Todos";

    [ObservableProperty]
    public partial int TotalUsuarios { get; set; }

    [ObservableProperty]
    public partial int TotalActivos { get; set; }

    [ObservableProperty]
    public partial int TotalInactivos { get; set; }

    public UsuariosConfigViewModel(IUsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
        Title = "Configuración - Usuarios";
    }

    public async Task InicializarAsync()
    {
        System.Diagnostics.Debug.WriteLine("[USUARIOS] Inicio inicialización");
        await CargarUsuariosAsync();
    }

    partial void OnBusquedaChanged(string value)
    {
        _ = CargarUsuariosAsync();
    }

    partial void OnFiltroEstadoChanged(string value)
    {
        _ = CargarUsuariosAsync();
    }

    [RelayCommand]
    private async Task CargarUsuariosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine("[USUARIOS] Inicio carga");

            bool? activo = FiltroEstado switch
            {
                "Activos" => true,
                "Inactivos" => false,
                _ => null,
            };

            var resultados = await _usuariosService.ObtenerUsuariosAsync(Busqueda, activo);

            System.Diagnostics.Debug.WriteLine($"[USUARIOS] Cantidad recibida: {resultados.Count}");

            Usuarios.Clear();
            foreach (var usuario in resultados)
            {
                Usuarios.Add(usuario);
            }

            TotalUsuarios = Usuarios.Count;
            TotalActivos = Usuarios.Count(x => x.Activo);
            TotalInactivos = Usuarios.Count(x => !x.Activo);

            System.Diagnostics.Debug.WriteLine("[USUARIOS] Fin carga");
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar los usuarios.";
            System.Diagnostics.Debug.WriteLine($"[USUARIOS][ERROR] Tipo: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[USUARIOS][ERROR] Mensaje: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[USUARIOS][ERROR] StackTrace: {ex.StackTrace}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NuevoAsync()
    {
        await Shell.Current.GoToAsync(nameof(UsuarioFormPage));
    }

    [RelayCommand]
    private async Task AbrirDetalleAsync(UsuarioListaItem usuario)
    {
        if (usuario is null) return;

        System.Diagnostics.Debug.WriteLine($"[USUARIO-DETALLE] Inicio navegación para {usuario.NombreCompleto}");

        var parametros = new Dictionary<string, object>
        {
            ["IdUsuario"] = usuario.IdUsuario,
            ["Usuario"] = usuario
        };

        await Shell.Current.GoToAsync(nameof(UsuarioDetallePage), parametros);
    }

    [RelayCommand]
    private async Task CambiarFiltroEstadoAsync()
    {
        var opcion = await Shell.Current.DisplayActionSheetAsync(
            "Filtrar por estado",
            "Cancelar",
            null,
            "Todos",
            "Activos",
            "Inactivos");

        if (string.IsNullOrEmpty(opcion) || opcion == "Cancelar") return;

        FiltroEstado = opcion;
        await CargarUsuariosAsync();
    }

    [RelayCommand]
    private async Task DescargarAsync()
    {
        var encabezados = new[] { "Usuario", "Nombre", "Correo", "Rol", "Estado" };
        var filas = Usuarios.Select(u => new[]
        {
            u.NombreUsuario,
            u.NombreCompleto,
            u.Email,
            u.RolesTexto,
            u.EstadoTexto
        }).ToList();

        await ReportService.SeleccionarYExportarAsync(
            "Reporte de Usuarios", encabezados, filas, "Usuarios");
    }
}
