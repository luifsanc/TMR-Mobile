namespace tmr_mobile.Models.Operaciones;

/// <summary>
/// Modelo de proyecto para visualización en la UI (ViewModel)
/// Se mapea desde ProyectoResponse del API
/// </summary>
public class ProyectoItem
{
    /// <summary>
    /// Identificador único del proyecto
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Código único del proyecto
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del proyecto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Cliente asociado
    /// </summary>
    public string Cliente { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual del proyecto
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Presupuesto del proyecto
    /// </summary>
    public decimal Presupuesto { get; set; }

    /// <summary>
    /// Horas totales
    /// </summary>
    public decimal Horas { get; set; }

    /// <summary>
    /// Nombre del líder del proyecto
    /// </summary>
    public string Lider { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de recursos asignados
    /// </summary>
    public int NumeroRecursos { get; set; }

    /// <summary>
    /// Tipo de proyecto
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio formateada (para display)
    /// </summary>
    public string FechaInicio { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de fin formateada (para display)
    /// </summary>
    public string FechaFin { get; set; } = string.Empty;

    /// <summary>
    /// Rango de fechas formateado como "dd/MM/yyyy - dd/MM/yyyy"
    /// </summary>
    public string FechaRango
    {
        get
        {
            if (string.IsNullOrEmpty(FechaInicio) || string.IsNullOrEmpty(FechaFin))
                return "Sin fecha especificada";

            return $"{FechaInicio} - {FechaFin}";
        }
    }

    /// <summary>
    /// Constructor vacío
    /// </summary>
    public ProyectoItem() { }

    /// <summary>
    /// Constructor con parámetros
    /// </summary>
    public ProyectoItem(
        int id,
        string codigo,
        string nombre,
        string cliente,
        string estado,
        decimal presupuesto,
        decimal horas,
        string lider,
        int numeroRecursos,
        string tipo,
        string fechaInicio,
        string fechaFin)
    {
        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Cliente = cliente;
        Estado = estado;
        Presupuesto = presupuesto;
        Horas = horas;
        Lider = lider;
        NumeroRecursos = numeroRecursos;
        Tipo = tipo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }
}
