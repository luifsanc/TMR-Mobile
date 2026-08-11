using CommunityToolkit.Mvvm.ComponentModel;

namespace tmr_mobile.Models;

public partial class ActividadCargaItem : ObservableObject
{
    public int Id { get; set; }
    public string Colaborador { get; set; } = string.Empty;
    public string Proyecto { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string LiderTecnico { get; set; } = string.Empty;
    public string FechaTexto { get; set; } = string.Empty;
    public string HorasTexto { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public decimal NroHoras { get; set; }
    public string Estado { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSeleccionado { get; set; }
}