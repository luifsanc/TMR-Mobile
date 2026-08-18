// tmr_shared/DTOs/Dashboard/HorasIncompletasResponse.cs
namespace tmr_shared.DTOs.Dashboard;

public class HorasIncompletasResponse
{
    public bool TieneFaltantes { get; set; }
    public decimal HorasFaltantes { get; set; }
    public List<DiaIncompletoResponse>? DiasIncompletos { get; set; }
}

public class DiaIncompletoResponse
{
    public DateOnly Fecha { get; set; }
    public decimal HorasRegistradas { get; set; }
    public decimal HorasFaltantes { get; set; }
}
