using System;
using System.Collections.Generic;
using tmr_mobile.Resources.Styles;

namespace tmr_mobile.Models.Seguimiento;

public class FiltroSeguimientoDto
{
    public string? Busqueda { get; set; }
    public string? ClienteSeleccionado { get; set; }
    public string FechaDesde { get; set; } = string.Empty;
    public string FechaHasta { get; set; } = string.Empty;
    public string? Periodo { get; set; }
}

public class SeguimientoColaboradorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Proyecto { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string LiderTecnico { get; set; } = string.Empty;
    public decimal NroHoras { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int DiasConReporte { get; set; }
    public int DiasACompletar { get; set; }

    // Helpers for UI
    public string Iniciales
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Nombre)) return "CO";
            var partes = Nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length >= 2)
            {
                return $"{partes[0][0]}{partes[1][0]}".ToUpper();
            }
            return partes[0].Length >= 2 ? partes[0].Substring(0, 2).ToUpper() : partes[0].ToUpper();
        }
    }

    public string ColorEstado => Estado switch
    {
        "Completo" => DesignColors.SuccessText,
        "En progreso" => DesignColors.WarningText,
        "Pendiente" => DesignColors.ErrorText,
        _ => DesignColors.TextMuted
    };

    public string ColorFondoEstado => Estado switch
    {
        "Completo" => DesignColors.SuccessSurface,
        "En progreso" => DesignColors.WarningSurface,
        "Pendiente" => DesignColors.ErrorSurface,
        _ => DesignColors.Secondary
    };
}
