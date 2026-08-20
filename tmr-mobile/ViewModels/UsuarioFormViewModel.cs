using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class UsuarioFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IUsuariosService _usuariosService;
    private readonly IColaboradoresService _colaboradoresService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    [NotifyPropertyChangedFor(nameof(TextoBoton))]
    public partial int? IdUsuario { get; set; }

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailPrefix { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UsuarioAlias { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool MostrarPassword { get; set; }

    [ObservableProperty]
    public partial bool DebeCambiarPassword { get; set; } = true;

    [ObservableProperty]
    public partial bool EsInterno { get; set; } = true;

    [ObservableProperty]
    public partial string TituloPagina { get; set; } = "Nuevo Usuario";

    [ObservableProperty]
    public partial bool Activo { get; set; } = true;

    [ObservableProperty]
    public partial ColaboradorModel? ColaboradorSeleccionado { get; set; }

    [ObservableProperty]
    public partial RolItem? RolSeleccionado { get; set; }

    public bool EsEdicion => IdUsuario.HasValue;
    public string TextoBoton => EsEdicion ? "Guardar" : "Guardar";

    public ObservableCollection<ColaboradorModel> Colaboradores { get; } = new();
    public ObservableCollection<RolItem> Roles { get; } = new();

    public string EmailFinal
    {
        get
        {
            if (EsInterno)
            {
                var prefijo = string.IsNullOrWhiteSpace(EmailPrefix) ? "" : EmailPrefix.Trim();
                return $"{prefijo}@integritysolutions.com.ec";
            }
            return Email.Trim();
        }
    }

    public UsuarioFormViewModel(IUsuariosService usuariosService, IColaboradoresService colaboradoresService)
    {
        _usuariosService = usuariosService;
        _colaboradoresService = colaboradoresService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Usuario", out var uObj) || query.TryGetValue("usuario", out uObj))
        {
            if (uObj is UsuarioDetalleItem det)
            {
                IdUsuario = det.IdUsuario;
                Email = det.Email;
                DebeCambiarPassword = det.DebeCambiarPassword;
                Activo = det.Activo;
                EsInterno = (det.IdPersona.HasValue && det.IdPersona > 0) || (!string.IsNullOrWhiteSpace(det.Email) && det.Email.EndsWith("@integritysolutions.com.ec", StringComparison.OrdinalIgnoreCase));
                if (EsInterno && !string.IsNullOrWhiteSpace(det.Email) && det.Email.Contains('@'))
                    EmailPrefix = det.Email.Split('@')[0];
                else
                    EmailPrefix = det.Email ?? string.Empty;
            }
            else if (uObj is UsuarioListaItem lista)
            {
                IdUsuario = lista.IdUsuario;
                Email = lista.Email;
                DebeCambiarPassword = lista.DebeCambiarPassword;
                Activo = lista.Activo;
                EsInterno = (lista.IdPersona.HasValue && lista.IdPersona > 0) || (!string.IsNullOrWhiteSpace(lista.Email) && lista.Email.EndsWith("@integritysolutions.com.ec", StringComparison.OrdinalIgnoreCase));
                if (EsInterno && !string.IsNullOrWhiteSpace(lista.Email) && lista.Email.Contains('@'))
                    EmailPrefix = lista.Email.Split('@')[0];
                else
                    EmailPrefix = lista.Email ?? string.Empty;
            }
        }

        if (query.TryGetValue("IdUsuario", out var idObj))
        {
            if (idObj is int id && id > 0) IdUsuario = id;
            else if (idObj is string idText && int.TryParse(idText, out var parsedId) && parsedId > 0) IdUsuario = parsedId;
        }

        TituloPagina = IdUsuario.HasValue ? "Editar Usuario" : "Nuevo Usuario";
        _ = CargarDatosInicialesAsync();
    }

    private async Task CargarDatosInicialesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // 1. Cargar lista de colaboradores activos
            var listaColaboradores = await _colaboradoresService.ObtenerColaboradoresAsync(activo: true);
            Colaboradores.Clear();
            foreach (var col in listaColaboradores)
            {
                Colaboradores.Add(col);
            }

            // 2. Cargar lista de roles activos
            var roles = await _usuariosService.ObtenerRolesAsync();
            Roles.Clear();
            foreach (var rol in roles)
            {
                if (rol.Activo)
                    Roles.Add(rol);
            }

            if (EsEdicion && IdUsuario.HasValue)
            {
                Password = string.Empty; // En edición la contraseña es opcional

                var detalle = await _usuariosService.ObtenerUsuarioAsync(IdUsuario.Value);
                if (detalle is not null)
                {
                    Email = detalle.Email;
                    UsuarioAlias = detalle.NombreUsuario;
                    DebeCambiarPassword = detalle.DebeCambiarPassword;
                    Activo = detalle.Activo;
                    EsInterno = (detalle.IdPersona.HasValue && detalle.IdPersona > 0) || (!string.IsNullOrWhiteSpace(detalle.Email) && detalle.Email.EndsWith("@integritysolutions.com.ec", StringComparison.OrdinalIgnoreCase));

                    if (EsInterno && !string.IsNullOrWhiteSpace(detalle.Email) && detalle.Email.Contains('@'))
                    {
                        EmailPrefix = detalle.Email.Split('@')[0];
                    }
                    else
                    {
                        EmailPrefix = detalle.Email ?? string.Empty;
                    }

                    // Seleccionar colaborador asignado por ID, Nombre o Email
                    if (EsInterno)
                    {
                        var colEncontrado = Colaboradores.FirstOrDefault(c =>
                            (detalle.IdPersona.HasValue && detalle.IdPersona > 0 && c.Id == detalle.IdPersona.Value) ||
                            (!string.IsNullOrWhiteSpace(detalle.NombreCompleto) && string.Equals(c.NombreCompleto, detalle.NombreCompleto, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrWhiteSpace(detalle.Email) && string.Equals(c.Email, detalle.Email, StringComparison.OrdinalIgnoreCase))
                        );

                        if (colEncontrado is not null)
                        {
                            ColaboradorSeleccionado = colEncontrado;
                        }
                    }

                    // Seleccionar rol asignado
                    var rolEncontrado = Roles.FirstOrDefault(r =>
                        detalle.Roles != null && detalle.Roles.Any(rolNombre => string.Equals(rolNombre, r.Nombre, StringComparison.OrdinalIgnoreCase)));
                    
                    RolSeleccionado = rolEncontrado ?? Roles.FirstOrDefault();
                }
            }
            else
            {
                RolSeleccionado = Roles.FirstOrDefault();
                RegenerarPassword();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al cargar el formulario del usuario.";
            System.Diagnostics.Debug.WriteLine($"[USUARIOS-FORM][ERROR] {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnColaboradorSeleccionadoChanged(ColaboradorModel? value)
    {
        if (value is null) return;

        if (!string.IsNullOrWhiteSpace(value.Email))
        {
            if (value.Email.Contains('@'))
            {
                EmailPrefix = value.Email.Split('@')[0];
            }
            else
            {
                EmailPrefix = value.Email;
            }
        }
        else if (!string.IsNullOrWhiteSpace(value.NombreCompleto))
        {
            var partes = value.NombreCompleto.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length >= 2)
                EmailPrefix = $"{partes[0]}.{partes[1]}";
            else if (partes.Length == 1)
                EmailPrefix = partes[0];
        }
    }

    partial void OnEsInternoChanged(bool value)
    {
        if (!value)
        {
            ColaboradorSeleccionado = null;
            if (string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(EmailPrefix))
            {
                Email = $"{EmailPrefix}@integritysolutions.com.ec";
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(Email) && Email.Contains('@'))
            {
                EmailPrefix = Email.Split('@')[0];
            }
        }
    }

    [RelayCommand]
    private void ToggleMostrarPassword()
    {
        MostrarPassword = !MostrarPassword;
    }

    [RelayCommand]
    private void RegenerarPassword()
    {
        var random = new Random();
        Password = $"Tmr{random.Next(100000, 999999)}!";
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (EsInterno)
        {
            if (ColaboradorSeleccionado is null && !EsEdicion)
            {
                ErrorMessage = "Debes seleccionar un colaborador para el usuario interno.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailPrefix))
            {
                ErrorMessage = "Ingresa el nombre del correo electrónico del usuario interno.";
                return;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@'))
            {
                ErrorMessage = "Ingresa un correo electrónico válido.";
                return;
            }
        }

        if (RolSeleccionado is null)
        {
            ErrorMessage = "Debes seleccionar un rol para el usuario.";
            return;
        }

        if (!EsEdicion && string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "La contraseña temporal es requerida.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var emailFinal = EmailFinal;

            int? idPersonaFinal = null;
            if (EsInterno && ColaboradorSeleccionado is not null)
            {
                idPersonaFinal = ColaboradorSeleccionado.IdPersona;
                if (!idPersonaFinal.HasValue || idPersonaFinal <= 0)
                {
                    var detalleCol = await _colaboradoresService.ObtenerColaboradorAsync(ColaboradorSeleccionado.Id);
                    idPersonaFinal = detalleCol?.IdPersona ?? ColaboradorSeleccionado.Id;
                }
            }

            if (EsEdicion && IdUsuario.HasValue)
            {
                var updateRequest = new UpdateUsuarioRequest
                {
                    IdPersona = idPersonaFinal,
                    Email = emailFinal,
                    Rolesids = RolSeleccionado is not null ? new List<int> { RolSeleccionado.Id } : new List<int>(),
                    DebeCambiarPassword = DebeCambiarPassword,
                    Password = string.IsNullOrWhiteSpace(Password) ? null : Password.Trim()
                };

                var resultado = await _usuariosService.ActualizarUsuarioAsync(IdUsuario.Value, updateRequest);
                if (resultado.Success)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Usuario actualizado",
                        "El usuario fue actualizado correctamente.",
                        "Aceptar");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = resultado.Message;
                }
            }
            else
            {
                var createRequest = new CreateUsuarioRequest
                {
                    IdPersona = idPersonaFinal,
                    Email = emailFinal,
                    Password = Password,
                    Rolesids = RolSeleccionado is not null ? new List<int> { RolSeleccionado.Id } : new List<int>(),
                    DebeCambiarPassword = DebeCambiarPassword
                };

                var resultado = await _usuariosService.CrearUsuarioAsync(createRequest);
                if (resultado.Success)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Usuario creado",
                        "El usuario fue creado correctamente.",
                        "Aceptar");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = resultado.Message;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al guardar el usuario.";
            System.Diagnostics.Debug.WriteLine($"[USUARIOS-FORM][ERROR] {ex.Message}");
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
