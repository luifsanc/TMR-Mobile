namespace tmr_mobile.Models.Operaciones;

public class ColaboradorModel
{
    public int Id { get; set; }
    public string CodigoEmpleado { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Asociacion { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public int NumProyectos { get; set; }
    public bool Activo { get; set; }

    public string Iniciales
    {
        get
        {
            if (string.IsNullOrWhiteSpace(NombreCompleto)) return "CO";
            var partes = NombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length >= 2)
            {
                return $"{partes[0][0]}{partes[1][0]}".ToUpper();
            }
            return partes[0].Length >= 2 ? partes[0].Substring(0, 2).ToUpper() : partes[0].ToUpper();
        }
    }

    public string EstadoTexto => Activo ? "ACTIVO" : "INACTIVO";
    public string ColorEstado => Activo ? "#43A047" : "#E53935";
    public string ColorFondoEstado => Activo ? "#E8F5E9" : "#FFEBEE";
}

public class ColaboradorDetalleModel : ColaboradorModel
{
    public int? IdEmpresaCatalogo { get; set; }
    public string? TipoPersona { get; set; }
    public int? IdTipoIdentificacion { get; set; }
    public int? IdGenero { get; set; }
    public int? IdNacionalidad { get; set; }
    public int? IdTipoContrato { get; set; }
    public int? IdModoTrabajo { get; set; }
    public int? IdCategoriaEmpleado { get; set; }
    public int? IdDepartamento { get; set; }
    public int? IdCargo { get; set; }

    public string TipoContrato { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string? FechaIngreso { get; set; }
    public string? FechaContratacion { get; set; }
    public int? AniosExperiencia { get; set; }
    public string Modalidad { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

    public int IdPersona { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? FechaNacimiento { get; set; }
    public string Genero { get; set; } = string.Empty;
    public string? Nacionalidad { get; set; }

    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    public List<ProyectoColaboradorModel> Proyectos { get; set; } = new();

    // Datos de Salida
    public string? FechaSalida { get; set; }
    public string? TipoSalida { get; set; }
    public string? CausaSalida { get; set; }
    public string? ComentarioSalida { get; set; }
    public string? ReemplazoNombre { get; set; }
    public string? ReemplazaANombre { get; set; }
}

public class ProyectoColaboradorModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    public string ColorEstado => Estado switch
    {
        "Completado" => "#43A047",
        "En progreso" => "#2563EB",
        "Pausado" or "En pausa" => "#F59E0B",
        "Cancelado" => "#E53935",
        _ => "#64748B"
    };

    public string ColorFondoEstado => Estado switch
    {
        "Completado" => "#E8F5E9",
        "En progreso" => "#EFF6FF",
        "Pausado" or "En pausa" => "#FEF3C7",
        "Cancelado" => "#FFEBEE",
        _ => "#F1F5F9"
    };
}

public class CreateColaboradorRequest
{
    public string TipoPersona { get; set; } = "NATURAL";
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public int IdTipoIdentificacion { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }

    public int IdEmpresaCatalogo { get; set; }
    public int IdTipoContrato { get; set; }

    public int IdDepartamento { get; set; }
    public int IdCargo { get; set; }
    public int? IdModoTrabajo { get; set; }
    public int? IdCategoriaEmpleado { get; set; }
    public int? AniosExperiencia { get; set; }

    public string? FechaContratacion { get; set; }
    public string? FechaNacimiento { get; set; }
    public int? IdGenero { get; set; }
    public int? IdNacionalidad { get; set; }
    public int? IdEmpleadoReemplazo { get; set; }
}

public class UpdateColaboradorRequest : CreateColaboradorRequest
{
    public bool Activo { get; set; }
}

public class RegistrarSalidaRequest
{
    public string FechaSalida { get; set; } = string.Empty;
    public int IdTipoSalida { get; set; }
    public int IdCausaSalida { get; set; }
    public string? Comentario { get; set; }
    public int? IdEmpleadoReemplazo { get; set; }
}

public class ColaboradorCatalogoItem
{
    public int Id { get; set; }
    public string Valor { get; set; } = string.Empty;
}

public class CargoItem
{
    public int Id { get; set; }
    public string NombreCargo { get; set; } = string.Empty;
    public int IdDepartamento { get; set; }
}
