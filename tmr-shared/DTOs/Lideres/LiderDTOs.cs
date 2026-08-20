namespace tmr_shared.DTOs.Lideres;

public class ProyectoAsignadoDTO
{
    public int? Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class LiderResponse
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Tipopersona { get; set; } = string.Empty;
    public int? Idtipo { get; set; }
    public string? TipoNombre { get; set; }
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime Fechacreacion { get; set; }
    public List<ProyectoAsignadoDTO> Proyectos { get; set; } = new();

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
    public string Inicial => !string.IsNullOrWhiteSpace(Nombres) ? Nombres.Substring(0, 1).ToUpper() : "?";
    public string TipoBadge
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(TipoNombre))
            {
                if (TipoNombre.ToLowerInvariant().Contains("interno")) return "Interno";
                if (TipoNombre.ToLowerInvariant().Contains("externo")) return "Externo";
                return TipoNombre;
            }
            return Tipopersona == "E" ? "Externo" : "Interno";
        }
    }
    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string ColorFondoEstado => Activo ? "#E8F5E9" : "#FFEBEE";
    public string ColorEstado => Activo ? "#2E7D32" : "#C62828";
    public string ClientesResumen => Proyectos.Count > 0 
        ? string.Join(", ", Proyectos.Select(p => p.Cliente).Where(c => !string.IsNullOrEmpty(c)).Distinct()) 
        : "Sin proyectos";
}

public class TipoLiderResponse
{
    public int Id { get; set; }
    public string Codigovalor { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class PersonaDisponibleResponse
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}

public class ContadoresLiderResponse
{
    public int Internos { get; set; }
    public int Externos { get; set; }
    public int Activos { get; set; }
    public int Inactivos { get; set; }
}

public class CrearLiderRequest
{
    public int? Idpersona { get; set; }
    public int Idtipo { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? NumeroIdentificacion { get; set; }
    public string Usuariocreacion { get; set; } = "mobile";
    public string Ipcreacion { get; set; } = "127.0.0.1";
}

public class ActualizarLiderRequest
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public int? Idtipo { get; set; }
    public bool Activo { get; set; }
    public string? NumeroIdentificacion { get; set; }
    public string Usuariomodificacion { get; set; } = "mobile";
    public string Ipmodificacion { get; set; } = "127.0.0.1";
}
