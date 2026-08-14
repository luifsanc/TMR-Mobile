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
    public string ColorFondoEstado => Activo ? "#E6FDEE" : "#F3F4F6";
    public string ColorTextoEstado => Activo ? "#16A34A" : "#6B7280";

    public string RecurrenciaTexto => EsRecurrente ? "Se repite cada año" : "No es recurrente";
    public string RecurrenciaIcono => EsRecurrente ? "🔁" : "⇄";

    public string TipoBadgeFondo => TipoFeriado switch
    {
        "Nacional" => "#EEF4FF",
        "Local" => "#FEF3C7",
        "Religioso" => "#F3E8FF",
        _ => "#F3F4F6"
    };

    public string TipoBadgeTexto => TipoFeriado switch
    {
        "Nacional" => "#163572",
        "Local" => "#D97706",
        "Religioso" => "#9333EA",
        _ => "#374151"
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
            if (EsHoy) return "#163572";
            if (!EsMesActual) return "#D1D5DB";
            return "#374151";
        }
    }

    public string ColorFondoDia
    {
        get
        {
            if (TieneFeriados) return "#E6FDEE";
            if (EsHoy) return "#F0F4FF";
            return "White";
        }
    }

    public string ColorBordeDia
    {
        get
        {
            if (EsHoy) return "#163572";
            if (TieneFeriados) return "#86EFAC";
            return "#E5E7EB";
        }
    }
}

public class CreateFeriadoRequest
{
    public string NombreFeriado { get; set; } = string.Empty;
    public DateTime FechaFeriado { get; set; } = DateTime.Today;
    public string TipoFeriado { get; set; } = "Nacional";
    public bool EsRecurrente { get; set; }
    public string? Descripcion { get; set; }
}

public class UpdateFeriadoRequest
{
    public string NombreFeriado { get; set; } = string.Empty;
    public DateTime FechaFeriado { get; set; } = DateTime.Today;
    public string TipoFeriado { get; set; } = "Nacional";
    public bool EsRecurrente { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
