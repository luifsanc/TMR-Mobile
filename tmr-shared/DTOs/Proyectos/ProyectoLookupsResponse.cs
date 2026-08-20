namespace tmr_shared.DTOs.Proyectos;

/// <summary>
/// Response del endpoint GET /api/proyectos/lookups
/// Contiene listas de valores para filtros y combos en formularios
/// </summary>
public record ProyectoLookupsResponse
{
    /// <summary>
    /// Lista de clientes disponibles
    /// </summary>
    public List<LookupItem> Clientes { get; init; } = new();

    /// <summary>
    /// Lista de líderes disponibles
    /// </summary>
    public List<LookupItem> Lideres { get; init; } = new();

    /// <summary>
    /// Lista de empleados/colaboradores disponibles
    /// </summary>
    public List<LookupItem> Empleados { get; init; } = new();

    /// <summary>
    /// Lista de estados posibles para proyectos
    /// </summary>
    public List<LookupItem> Estados { get; init; } = new();

    /// <summary>
    /// Lista de tipos de proyectos
    /// </summary>
    public List<LookupItem> Tipos { get; init; } = new();
}

/// <summary>
/// Elemento básico para lookups (Id + Nombre)
/// </summary>
public record LookupItem
{
    /// <summary>
    /// Identificador del elemento
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Nombre o descripción del elemento
    /// </summary>
    public string Nombre { get; init; } = string.Empty;

    /// <summary>
    /// Departamento del colaborador, cuando el elemento pertenece al catálogo de empleados.
    /// </summary>
    public string Departamento { get; init; } = string.Empty;

    public string NombreConDepartamento =>
        string.IsNullOrWhiteSpace(Departamento)
            ? Nombre
            : $"{Nombre} - {Departamento}";
}
