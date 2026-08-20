using tmr_mobile.Resources.Styles;

namespace tmr_mobile.Models.Configuracion;

public class FeriadoItem
{
    public int Id { get; set; }
    public string NombreFeriado { get; set; } = string.Empty;
    public string Nombre
    {
        get => NombreFeriado;
        set => NombreFeriado = value;
    }

    public DateTime FechaFeriado { get; set; } = DateTime.Today;
    public DateTime Fecha
    {
        get => FechaFeriado;
        set => FechaFeriado = value;
    }

    public string TipoFeriado { get; set; } = "Nacional";
    public string Tipo
    {
        get => TipoFeriado;
        set => TipoFeriado = value;
    }

    public bool EsRecurrente { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string ColorFondoEstado => Activo ? DesignColors.SuccessSurface : DesignColors.Secondary;
    public string ColorTextoEstado => Activo ? DesignColors.SuccessText : DesignColors.TextMuted;

    public string RecurrenciaTexto => EsRecurrente ? "Se repite cada año" : "No es recurrente";
    public string RecurrenciaIcono => EsRecurrente ? "🔁" : "⇄";

    public string TipoBadgeFondo => TipoFeriado switch
    {
        "Nacional" => DesignColors.PrimaryLight,
        "Local" => DesignColors.WarningSurface,
        "Religioso" => DesignColors.PurpleSurface,
        _ => DesignColors.Secondary
    };

    public string TipoBadgeTexto => TipoFeriado switch
    {
        "Nacional" => DesignColors.Primary,
        "Local" => DesignColors.WarningText,
        "Religioso" => DesignColors.PurpleText,
        _ => DesignColors.TextMain
    };

    public string FechaTextoFormat => FechaFeriado.ToString("dd/MM/yyyy");
}

public class DiaCalendarioItem
{
    public int NumeroDia { get; set; }
    public DateTime Fecha { get; set; }
    public bool EsMesActual { get; set; }
    public bool EsHoy { get; set; }
    public List<FeriadoItem> Feriados { get; set; } = new();
    public bool TieneFeriados => Feriados.Count > 0;
    public FeriadoItem? PrimerFeriado => Feriados.FirstOrDefault();

    public string ColorNumeroDia
    {
        get
        {
            if (EsHoy) return DesignColors.Primary;
            if (!EsMesActual) return DesignColors.Border;
            return DesignColors.TextMain;
        }
    }

    public string ColorFondoDia
    {
        get
        {
            if (TieneFeriados) return DesignColors.SuccessSurface;
            if (EsHoy) return DesignColors.PrimaryLight;
            return DesignColors.Surface;
        }
    }

    public string ColorBordeDia
    {
        get
        {
            if (EsHoy) return DesignColors.Primary;
            if (TieneFeriados) return DesignColors.SuccessText;
            return DesignColors.Border;
        }
    }
}

public class CreateFeriadoRequest
{
    public string NombreFeriado { get; set; } = string.Empty;
    public string FechaFeriado { get; set; } = string.Empty;
    public string TipoFeriado { get; set; } = "Nacional";
    public bool EsRecurrente { get; set; }
    public string? Descripcion { get; set; }
}

public class UpdateFeriadoRequest
{
    public string NombreFeriado { get; set; } = string.Empty;
    public string FechaFeriado { get; set; } = string.Empty;
    public string TipoFeriado { get; set; } = "Nacional";
    public bool EsRecurrente { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
