using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using tmr_mobile.Services;
using System.Collections.Generic;
using System.Linq;
using tmr_mobile.Resources.Styles;

namespace tmr_mobile.ViewModels;

public class ModuloItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string IconoOscuro => ThemeImages.DarkVariant(Icono);
    public string Ruta { get; set; } = string.Empty;
}

public partial class HomeViewModel : ObservableObject
{
    private readonly IUserModuleAccessService _moduleAccessService;
    private HashSet<string> _modulosPermitidos = new(StringComparer.OrdinalIgnoreCase);

    private static readonly ModuloItem[] CatalogoModulos =
    [
        new() { Nombre = "Proyectos", Icono = "icon_proyectos.png", Ruta = "ProyectosPage" },
        new() { Nombre = "Time Report", Icono = "icon_timereport.png", Ruta = "TimeReportPage" },
        new() { Nombre = "Carga Actividades", Icono = "icon_carga.png", Ruta = "CargaActividadesPage" },
        new() { Nombre = "Reportes", Icono = "icon_reportes.png", Ruta = "ReportesPage" },
        new() { Nombre = "Líderes", Icono = "icon_lideres.png", Ruta = "LideresPage" },
        new() { Nombre = "Colaboradores", Icono = "icon_colaboradores.png", Ruta = "ColaboradoresPage" },
        new() { Nombre = "Clientes", Icono = "icon_clientes.png", Ruta = "ClientesPage" },
        new() { Nombre = "Seguimiento", Icono = "icon_seguimiento.png", Ruta = "SeguimientoPage" },
        new() { Nombre = "Usuarios", Icono = "user_profile.png", Ruta = "UsuariosPage" },
        new() { Nombre = "Roles", Icono = "rol.png", Ruta = "RolesPage" },
        new() { Nombre = "Feriados", Icono = "feriado.png", Ruta = "FeriadosPage" },
        new() { Nombre = "Catálogos", Icono = "catalogo.png", Ruta = "CatalogosPage" }
    ];

    public ObservableCollection<ModuloItem> Modulos { get; } = new();

    [ObservableProperty]
    public partial bool EsColaborador { get; set; }

    public bool MostrarEncabezado => !EsColaborador;

    partial void OnEsColaboradorChanged(bool value)
    {
        OnPropertyChanged(nameof(MostrarEncabezado));
    }

    public HomeViewModel(IUserModuleAccessService moduleAccessService)
    {
        _moduleAccessService = moduleAccessService;
        _ = CargarModulosAsync();
    }

    private async Task CargarModulosAsync()
    {
        Modulos.Clear();

        try
        {
            EsColaborador = await _moduleAccessService.EsColaboradorAsync();
            _modulosPermitidos = await _moduleAccessService.ObtenerModulosAsync();

            foreach (var modulo in CatalogoModulos)
            {
                if (TieneAcceso(modulo.Ruta))
                {
                    Modulos.Add(modulo);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los módulos autorizados: {ex.Message}");
        }
    }

    private bool TieneAcceso(string ruta)
    {
        return ruta switch
        {
            "ProyectosPage" => TieneModulo("Proyectos"),
            "TimeReportPage" => TieneModulo("Actividades") || TieneModulo("Time Report"),
            "CargaActividadesPage" => TieneModulo("Carga Actividades"),
            "ReportesPage" => TieneModulo("Reportes") || TieneModulo("Proyecto por horas") || TieneModulo("Proyecto por fechas"),
            "LideresPage" => TieneModulo("Lideres"),
            "ColaboradoresPage" => TieneModulo("Colaboradores"),
            "ClientesPage" => TieneModulo("Clientes"),
            "SeguimientoPage" => TieneModulo("Seguimiento"),
            "UsuariosPage" => TieneModulo("Usuarios"),
            "RolesPage" => TieneModulo("Roles"),
            "FeriadosPage" => TieneModulo("Dias Festivos"),
            "CatalogosPage" => TieneModulo("Configuracion"),
            _ => false
        };
    }

    private bool TieneModulo(string nombre) => _modulosPermitidos.Contains(nombre);

    [RelayCommand]
    private async Task NavigateAsync(string route)
    {
        if (!string.IsNullOrEmpty(route) && TieneAcceso(route))
        {
            // Todos los módulos principales ahora son ShellItems raíz
            while (Shell.Current.Navigation.ModalStack.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync(false);
            }

            await Shell.Current.GoToAsync($"//{route}");
        }
    }
}
