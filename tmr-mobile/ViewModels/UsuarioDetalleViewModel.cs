using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile.ViewModels;

public partial class UsuarioDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IUsuariosService _usuariosService;
    private int _idUsuario;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsActivo))]
    [NotifyPropertyChangedFor(nameof(EsInactivo))]
    [NotifyPropertyChangedFor(nameof(TextoBotonEstado))]
    [NotifyPropertyChangedFor(nameof(RolesTexto))]
    [NotifyPropertyChangedFor(nameof(EstadoDetalleTexto))]
    [NotifyPropertyChangedFor(nameof(NombreMostrado))]
    public partial UsuarioDetalleItem? Usuario { get; set; }

    public bool EsActivo => Usuario?.Activo == true;
    public bool EsInactivo => Usuario != null && !Usuario.Activo;
    public string TextoBotonEstado => EsActivo ? "Desactivar" : "Activar";
    public string RolesTexto => Usuario?.Roles is null || Usuario.Roles.Count == 0 ? "Sin asignación" : string.Join(", ", Usuario.Roles);
    public string EstadoDetalleTexto => Usuario?.Activo == true ? "Activo" : "Inactivo";
    public string NombreMostrado => Usuario?.NombreCompleto ?? "Usuario";

    public UsuarioDetalleViewModel(IUsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
        Title = "Detalle de usuario";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        System.Diagnostics.Debug.WriteLine("[USUARIO-DETALLE] Inicio ApplyQueryAttributes");

        if (query.TryGetValue("Usuario", out var uObj) || query.TryGetValue("usuario", out uObj))
        {
            if (uObj is UsuarioDetalleItem det)
            {
                Usuario = det;
                _idUsuario = det.IdUsuario;
            }
            else if (uObj is UsuarioListaItem lista)
            {
                Usuario = new UsuarioDetalleItem
                {
                    Id = lista.Id,
                    IdUsuario = lista.IdUsuario,
                    IdPersona = lista.IdPersona,
                    NumeroIdentificacion = lista.NumeroIdentificacion,
                    Nombres = lista.Nombres,
                    Apellidos = lista.Apellidos,
                    Email = lista.Email,
                    Roles = lista.Roles,
                    Activo = lista.Activo,
                    DebeCambiarPassword = lista.DebeCambiarPassword,
                    UltimoLogin = lista.UltimoLogin
                };
                _idUsuario = lista.IdUsuario;
            }
        }

        var idValue = query.TryGetValue("IdUsuario", out var idObj)
            ? idObj
            : query.TryGetValue("idUsuario", out var idObjLower)
                ? idObjLower
                : query.TryGetValue("id", out var idObjSimple)
                    ? idObjSimple
                    : null;

        if (idValue is int id && id > 0)
        {
            _idUsuario = id;
        }
        else if (idValue is string idText && int.TryParse(idText, out var parsedId) && parsedId > 0)
        {
            _idUsuario = parsedId;
        }

        if (_idUsuario > 0)
        {
            _ = CargarDetalleAsync();
        }
        else if (Usuario is null)
        {
            ErrorMessage = "No se pudo identificar el usuario para mostrar su detalle.";
        }
    }

    [RelayCommand]
    private async Task CargarDetalleAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine($"[USUARIO-DETALLE] Inicio carga para ID {_idUsuario}");
            var resultado = await _usuariosService.ObtenerUsuarioAsync(_idUsuario);

            if (resultado is not null)
            {
                Usuario = resultado;
            }
            else if (Usuario is null)
            {
                ErrorMessage = "No se pudo cargar el detalle del usuario.";
            }
        }
        catch (Exception ex)
        {
            if (Usuario is null)
            {
                ErrorMessage = "Ocurrió un error al cargar el detalle.";
            }
            System.Diagnostics.Debug.WriteLine($"[USUARIO-DETALLE][ERROR] {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditarAsync()
    {
        if (Usuario is null) return;

        var parameters = new Dictionary<string, object>
        {
            ["IdUsuario"] = Usuario.IdUsuario,
            ["Usuario"] = Usuario
        };

        await Shell.Current.GoToAsync(nameof(UsuarioFormPage), parameters);
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync()
    {
        if (Usuario is null || IsBusy) return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmación",
            $"¿Deseas {(EsActivo ? "desactivar" : "activar")} a {Usuario.NombreCompleto.Trim()}?",
            "Sí",
            "No");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            var success = await _usuariosService.CambiarEstadoAsync(Usuario.IdUsuario, !Usuario.Activo);

            if (success)
            {
                Usuario.Activo = !Usuario.Activo;
                OnPropertyChanged(nameof(Usuario));
                OnPropertyChanged(nameof(EsActivo));
                OnPropertyChanged(nameof(EsInactivo));
                OnPropertyChanged(nameof(TextoBotonEstado));

                await Shell.Current.DisplayAlertAsync(
                    "Estado actualizado",
                    $"El usuario quedó {(EsActivo ? "activo" : "inactivo")}.",
                    "Aceptar");
            }
            else
            {
                ErrorMessage = "No se pudo cambiar el estado del usuario.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudo cambiar el estado.";
            System.Diagnostics.Debug.WriteLine($"[USUARIO-DETALLE][ERROR] {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegresarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
