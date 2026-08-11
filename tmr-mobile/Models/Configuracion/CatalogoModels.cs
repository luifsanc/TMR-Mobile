using CommunityToolkit.Mvvm.ComponentModel;

namespace tmr_mobile.Models.Configuracion;

public partial class CatalogoMaster : ObservableObject
{
    public int Id { get; set; }

    public string TipoCatalogo { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public string NombreMostrar =>
        !string.IsNullOrWhiteSpace(Descripcion)
            ? Descripcion
            : TipoCatalogo;
}

public class CatalogoDetalle
{
    public int Id { get; set; }

    public int IdCatalogo { get; set; }

    public string CodigoValor { get; set; } = string.Empty;

    public string Valor { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public short? Orden { get; set; }

    public string? ValorExtra { get; set; }

    public bool Activo { get; set; }

    public string ColorEstado =>
        Activo
            ? "#43A047"
            : "#E53935";

    public bool TieneDescripcion =>
        !string.IsNullOrWhiteSpace(Descripcion);
}

public class CreateCatalogoDetalleRequest
{
    public int IdCatalogo { get; set; }

    public string CodigoValor { get; set; } = string.Empty;

    public string Valor { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public short? Orden { get; set; }

    public string? ValorExtra { get; set; }
}

public class UpdateCatalogoDetalleRequest
{
    public string Valor { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public short? Orden { get; set; }

    public string? ValorExtra { get; set; }

    public bool? Activo { get; set; }

    public int? IdCatalogo { get; set; }

    public string? CodigoValor { get; set; }
}