namespace tmr_mobile.Models.Operaciones;

public class ColaboradorModel
{
    public int Id { get; set; }
    public int? IdPersona { get; set; }
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

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string ColorEstado => Activo ? "#16A34A" : "#6B7280";
    public string ColorFondoEstado => Activo ? "#DCFCE7" : "#F3F4F6";

    public string IdentificacionTexto => !string.IsNullOrWhiteSpace(NumeroIdentificacion) ? NumeroIdentificacion : "Sin identificación";
    public string EmailTexto => !string.IsNullOrWhiteSpace(Email) ? Email : "Sin correo";
    public string CargoTexto => !string.IsNullOrWhiteSpace(Cargo) ? Cargo : "Sin cargo";
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
    public int? IdEmpleadoReemplazo { get; set; }

    public string TipoContrato { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string? FechaIngreso { get; set; }
    public string? FechaContratacion { get; set; }
    public int? AniosExperiencia { get; set; }
    public string Modalidad { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

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

    // Helpers de formateo para vista de detalles
    public string EmpresaTexto => !string.IsNullOrWhiteSpace(Asociacion) ? Asociacion : "—";
    public string TipoContratoTexto => !string.IsNullOrWhiteSpace(TipoContrato) ? TipoContrato : "—";
    public string DepartamentoTexto => !string.IsNullOrWhiteSpace(Departamento) ? Departamento : "—";
    public string CargoDetalleTexto => !string.IsNullOrWhiteSpace(Cargo) ? Cargo : "—";
    public string ModalidadTexto => !string.IsNullOrWhiteSpace(Modalidad) ? Modalidad : "—";
    public string CategoriaTexto => !string.IsNullOrWhiteSpace(Categoria) ? Categoria : "—";
    public string AniosExperienciaTexto => AniosExperiencia.HasValue ? $"{AniosExperiencia.Value} años" : "—";

    public string TipoPersonaTexto => !string.IsNullOrWhiteSpace(TipoPersona) ? TipoPersona : "NATURAL";
    public string GeneroTexto => !string.IsNullOrWhiteSpace(Genero) ? Genero : "—";
    public string NacionalidadTexto => !string.IsNullOrWhiteSpace(Nacionalidad) ? Nacionalidad : "—";
    public string TelefonoTexto => !string.IsNullOrWhiteSpace(Telefono) ? Telefono : "—";
    public string DireccionTexto => !string.IsNullOrWhiteSpace(Direccion) ? Direccion : "—";

    public string FechaContratacionFormateada
    {
        get
        {
            var fechaStr = FechaContratacion ?? FechaIngreso;
            if (DateTime.TryParse(fechaStr, out var d)) return d.ToString("dd/MM/yyyy");
            return !string.IsNullOrWhiteSpace(fechaStr) ? fechaStr : "—";
        }
    }

    public string FechaNacimientoFormateada
    {
        get
        {
            if (DateTime.TryParse(FechaNacimiento, out var d)) return d.ToString("dd/MM/yyyy");
            return !string.IsNullOrWhiteSpace(FechaNacimiento) ? FechaNacimiento : "—";
        }
    }

    public string FechaSalidaFormateada
    {
        get
        {
            if (DateTime.TryParse(FechaSalida, out var d)) return d.ToString("dd/MM/yyyy");
            return !string.IsNullOrWhiteSpace(FechaSalida) ? FechaSalida : "—";
        }
    }

    public bool TieneReemplazoInfo => !string.IsNullOrWhiteSpace(ReemplazoNombre) || !string.IsNullOrWhiteSpace(ReemplazaANombre);
    public bool TieneSalidaInfo => !string.IsNullOrWhiteSpace(FechaSalida) || !string.IsNullOrWhiteSpace(TipoSalida) || !string.IsNullOrWhiteSpace(CausaSalida);
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
    public string? FechaIngreso { get; set; }
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
