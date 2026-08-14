using CommunityToolkit.Mvvm.ComponentModel;

namespace tmr_mobile.Models.Configuracion;

public class RolItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public class UsuarioListaItem
{
    private int _idUsuario;

    public int Id { get; set; }

    public int IdUsuario
    {
        get => _idUsuario > 0 ? _idUsuario : Id;
        set => _idUsuario = value;
    }

    public int? IdPersona { get; set; }
    public string? NumeroIdentificacion { get; set; }
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool Activo { get; set; } = true;
    public bool DebeCambiarPassword { get; set; }
    public DateTime? UltimoLogin { get; set; }

    public string NombreCompleto
    {
        get
        {
            var full = string.Join(' ', new[] { Nombres, Apellidos }.Where(v => !string.IsNullOrWhiteSpace(v))).Trim();
            return string.IsNullOrWhiteSpace(full) ? (string.IsNullOrWhiteSpace(Email) ? "Usuario" : Email.Split('@')[0]) : full;
        }
    }

    public string NombreUsuario =>
        string.IsNullOrWhiteSpace(Email) ? "usuario" : Email.Split('@')[0];

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string EstadoColor => Activo ? "#16A34A" : "#6B7280";
    public string ColorFondoEstado => Activo ? "#E6FDEE" : "#F3F4F6";
    public string ColorTextoEstado => Activo ? "#16A34A" : "#6B7280";

    public string Iniciales
    {
        get
        {
            var partes = (NombreCompleto ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length >= 2) return $"{partes[0][0]}{partes[1][0]}".ToUpper();
            if (partes.Length == 1 && partes[0].Length > 0) return partes[0][..Math.Min(2, partes[0].Length)].ToUpper();
            if (!string.IsNullOrWhiteSpace(NombreUsuario)) return NombreUsuario[..Math.Min(2, NombreUsuario.Length)].ToUpper();
            return "US";
        }
    }

    public string RolesTexto => Roles is null || Roles.Count == 0 ? "Sin asignación" : string.Join(", ", Roles);
}

public class UsuarioDetalleItem : UsuarioListaItem
{
    public bool EsInterno { get; set; } = true;
    public string TipoUsuarioTexto => (IdPersona.HasValue && IdPersona > 0) || EsInterno ? "Interno" : "Externo";

    public int? IdTipoIdentificacion { get; set; }
    public string? TipoIdentificacionValor { get; set; }
    public int? IdGenero { get; set; }
    public string? GeneroValor { get; set; }
    public int? IdNacionalidad { get; set; }
    public string? NacionalidadValor { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = string.Empty;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
}

public class UsuarioListadoEnvelope
{
    public List<UsuarioListaItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UsuarioRolesEnvelope
{
    public List<RolItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class CreateUsuarioRequest
{
    public int? IdPersona { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<int> Rolesids { get; set; } = new();
    public bool DebeCambiarPassword { get; set; } = true;
}

public class UpdateUsuarioRequest
{
    public int? IdPersona { get; set; }
    public List<int>? Rolesids { get; set; }
    public string? Email { get; set; }
    public string? NombreUsuario { get; set; }
    public string? Password { get; set; }
    public bool? DebeCambiarPassword { get; set; }
}

public class CambiarEstadoUsuarioRequest
{
    public bool Activo { get; set; }
}
