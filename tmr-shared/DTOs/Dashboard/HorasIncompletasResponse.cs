// tmr_shared/DTOs/Dashboard/HorasIncompletasResponse.cs
namespace tmr_shared.DTOs.Dashboard;

public class HorasIncompletasResponse
{
    public bool TieneFaltantes { get; set; }
    public decimal HorasFaltantes { get; set; }
    public List<DiaIncompletoResponse>? DiasIncompletos { get; set; }
}

// El array vino vacío en la prueba, así que no conozco aún sus campos.
// Cuando encuentres un caso con datos (un colaborador con horas
// pendientes), pásame el JSON y completo esta clase.
public class DiaIncompletoResponse
{
}