using CommunityToolkit.Mvvm.ComponentModel;

namespace tmr_mobile.Models.Operaciones;

public class ClienteModel
{
    public int Id { get; set; }
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string ColorEstado => Activo ? "#43A047" : "#E53935";
    public string ColorFondoEstado => Activo ? "#E8F5E9" : "#FFEBEE";
}

public class ClienteDetalleModel : ClienteModel
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    
    public List<ProyectoClienteModel> Proyectos { get; set; } = new();
}

public class ProyectoClienteModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class CreateClienteRequest
{
    public int IdTipoIdentificacion { get; set; }
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}

public class UpdateClienteRequest : CreateClienteRequest
{
    public bool Activo { get; set; }
}

public class TipoIdentificacionModel
{
    public int Id { get; set; }
    public string Valor { get; set; } = string.Empty;
}
