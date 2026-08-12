using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class CatalogoFormViewModel :
    BaseViewModel,
    IQueryAttributable
{
    private readonly ICatalogosService _catalogosService;

    private CatalogoDetalle? _detalleOriginal;

    [ObservableProperty]
    public partial int IdCatalogo { get; set; }

    [ObservableProperty]
    public partial int IdDetalle { get; set; }

    [ObservableProperty]
    public partial string NombreCatalogo { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial string CodigoValor { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial string Valor { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial string Descripcion { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial string OrdenTexto { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial string ValorExtra { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial bool Activo { get; set; } = true;

    [ObservableProperty]
    public partial bool EsEdicion { get; set; }

    public string TituloFormulario =>
        EsEdicion
            ? "Editar registro"
            : "Nuevo registro";

    public string TextoBoton =>
        EsEdicion
            ? "Guardar cambios"
            : "Crear registro";

    public CatalogoFormViewModel(
        ICatalogosService catalogosService)
    {
        _catalogosService = catalogosService;
    }

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (
            query.TryGetValue(
                "Catalogo",
                out var catalogoValue)
            &&
            catalogoValue is CatalogoMaster catalogo
        )
        {
            IdCatalogo = catalogo.Id;
            NombreCatalogo = catalogo.NombreMostrar;
        }

        if (
            query.TryGetValue(
                "Detalle",
                out var detalleValue)
            &&
            detalleValue is CatalogoDetalle detalle
        )
        {
            _detalleOriginal = detalle;

            EsEdicion = true;

            IdDetalle = detalle.Id;
            IdCatalogo = detalle.IdCatalogo;
            CodigoValor = detalle.CodigoValor;
            Valor = detalle.Valor;
            Descripcion = detalle.Descripcion ?? string.Empty;
            OrdenTexto = detalle.Orden?.ToString() ?? string.Empty;
            ValorExtra = detalle.ValorExtra ?? string.Empty;
            Activo = detalle.Activo;
        }
        else
        {
            EsEdicion = false;
            Activo = true;
        }

        OnPropertyChanged(nameof(TituloFormulario));
        OnPropertyChanged(nameof(TextoBoton));
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (IsBusy)
            return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Valor))
        {
            ErrorMessage =
                "Ingrese el nombre o valor del registro.";

            return;
        }

        if (
            !EsEdicion &&
            string.IsNullOrWhiteSpace(CodigoValor)
        )
        {
            ErrorMessage =
                "Ingrese el código.";

            return;
        }

        short? orden = null;

        if (!string.IsNullOrWhiteSpace(OrdenTexto))
        {
            if (
                !short.TryParse(
                    OrdenTexto,
                    out var ordenConvertido)
            )
            {
                ErrorMessage =
                    "El orden debe ser un número válido.";

                return;
            }

            orden = ordenConvertido;
        }

        try
        {
            IsBusy = true;

            if (EsEdicion)
            {
                var request =
                    new UpdateCatalogoDetalleRequest
                    {
                        IdCatalogo = IdCatalogo,

                        CodigoValor =
                            CodigoValor
                                .Trim()
                                .ToUpperInvariant(),

                        Valor =
                            Valor.Trim(),

                        Descripcion =
                            TextoOpcional(
                                Descripcion
                            ),

                        Orden = orden,

                        ValorExtra =
                            TextoOpcional(
                                ValorExtra
                            ),

                        Activo = Activo
                    };

                var resultado =
                    await _catalogosService
                        .ActualizarDetalleAsync(
                            IdDetalle,
                            request
                        );

                if (resultado is null)
                {
                    ErrorMessage =
                        "No se pudo actualizar el registro.";

                    return;
                }
            }
            else
            {
                var request =
                    new CreateCatalogoDetalleRequest
                    {
                        IdCatalogo = IdCatalogo,

                        CodigoValor =
                            CodigoValor
                                .Trim()
                                .ToUpperInvariant(),

                        Valor =
                            Valor.Trim(),

                        Descripcion =
                            TextoOpcional(
                                Descripcion
                            ),

                        Orden = orden,

                        ValorExtra =
                            TextoOpcional(
                                ValorExtra
                            )
                    };

                var resultado =
                    await _catalogosService
                        .CrearDetalleAsync(
                            request
                        );

                if (resultado is null)
                {
                    ErrorMessage =
                        "No se pudo crear el registro.";

                    return;
                }
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "Ocurrió un error al guardar.";

            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegresarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private static string? TextoOpcional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}