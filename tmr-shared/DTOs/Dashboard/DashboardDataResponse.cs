namespace tmr_shared.DTOs.Dashboard;

public record DashboardDataResponse
{
    public DashboardMetricasResponse? Metricas { get; init; }
}

public record DashboardMetricasResponse
{
    public int TotalProyectos { get; init; }
    public int ColaboradoresActivos { get; init; }
    public decimal HorasReportadas { get; init; }
    public int ClientesActivos { get; init; }
}