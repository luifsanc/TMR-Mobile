namespace tmr_shared.DTOs.TimeReport;
using System;

public record CalendarioActividadDto(
    int Id,
    int IdEmpleado,
    int? IdProyecto,
    string ProyectoNombre,
    int IdTipoActividad,
    string TipoActividadNombre,
    string? CodigoRequerimiento,
    decimal CantidadHoras,
    DateOnly FechaActividad,
    string DescripcionActividad,
    string? Notas,
    bool? EsBillable
);