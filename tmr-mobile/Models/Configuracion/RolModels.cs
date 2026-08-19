using tmr_mobile.Resources.Styles;

namespace tmr_mobile.Models.Configuracion;

public class RolModuloItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public bool Selected { get; set; }
}

public class RolListaItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public List<int> Modulosids { get; set; } = new();
    public List<RolModuloItem> Modulos { get; set; } = new();
    public bool Activo { get; set; } = true;

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string EstadoColor => Activo ? DesignColors.SuccessText : DesignColors.TextMuted;
    public string ColorFondoEstado => Activo ? DesignColors.SuccessSurface : DesignColors.Secondary;
    public string ColorTextoEstado => Activo ? DesignColors.SuccessText : DesignColors.TextMuted;

    public string Iniciales
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Nombre)) return "RO";
            var partes = Nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length >= 2)
            {
                return $"{partes[0][0]}{partes[1][0]}".ToUpper();
            }
            return partes[0].Length >= 2 ? partes[0].Substring(0, 2).ToUpper() : partes[0].ToUpper();
        }
    }

    public List<string> ModulosNombres
    {
        get
        {
            if (Modulos is not null && Modulos.Count > 0)
                return Modulos.Select(m => m.Nombre).ToList();
            return new List<string>();
        }
    }

    public string ModulosTexto => ModulosNombres.Count == 0 ? "Sin módulos" : string.Join(", ", ModulosNombres);
    public string PrimerosModulosTexto => ModulosNombres.Count == 0 ? "Sin módulos" : string.Join(", ", ModulosNombres.Take(3));
    public int ModulosRestantesCount => Math.Max(0, ModulosNombres.Count - 3);
    public bool TieneModulosRestantes => ModulosRestantesCount > 0;
    public string ModulosRestantesTexto => $"+{ModulosRestantesCount} más";
}

public class RolListadoEnvelope
{
    public List<RolListaItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class ModuloListadoEnvelope
{
    public List<RolModuloItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class CreateRolRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public List<int> Modulosids { get; set; } = new();
}

public class UpdateRolRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public List<int> Modulosids { get; set; } = new();
    public bool? Activo { get; set; }
}

public class CambiarEstadoRolRequest
{
    public bool Activo { get; set; }
}
