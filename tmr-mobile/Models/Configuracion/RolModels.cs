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
    public string EstadoColor => Activo ? "#16A34A" : "#6B7280";
    public string ColorFondoEstado => Activo ? "#E6FDEE" : "#F3F4F6";
    public string ColorTextoEstado => Activo ? "#16A34A" : "#6B7280";

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
