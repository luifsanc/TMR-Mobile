namespace tmr_shared.DTOs.TimeReport;

public record ResumenHorasDto(
    decimal HorasPorRegistrar,
    decimal HorasRegistradas,
    decimal HorasSemana,
    decimal HorasMes
);
