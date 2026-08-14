using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Resources.Styles;
using tmr_mobile.Services;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile.ViewModels;

public partial class CatalogoDetalleViewModel :
    BaseViewModel,
    IQueryAttributable
{
    private readonly ICatalogosService _catalogosService;
    private readonly IConfirmDialogService _confirmDialogService;

    [ObservableProperty]
    public partial CatalogoDetalle? Detalle { get; set; }

    [ObservableProperty]
    public partial CatalogoMaster? Catalogo { get; set; }

    public string EstadoTexto =>
        Detalle?.Activo == true
            ? "Activo"
            : "Inactivo";

    public string EstadoColor =>
        Detalle?.Activo == true
            ? DesignColors.Accent
            : DesignColors.Error;

    public bool TieneDescripcion =>
        !string.IsNullOrWhiteSpace(
            Detalle?.Descripcion
        );

    public bool TieneOrden =>
        Detalle?.Orden is not null;

    public bool TieneValorExtra =>
        !string.IsNullOrWhiteSpace(
            Detalle?.ValorExtra
        );

    public CatalogoDetalleViewModel(
        ICatalogosService catalogosService,
        IConfirmDialogService confirmDialogService)
    {
        _catalogosService = catalogosService;
        _confirmDialogService = confirmDialogService;

        Title = "Detalle";
    }

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (
            query.TryGetValue(
                "Detalle",
                out var detalleValue)
            &&
            detalleValue is CatalogoDetalle detalle
        )
        {
            Detalle = detalle;
        }

        if (
            query.TryGetValue(
                "Catalogo",
                out var catalogoValue)
            &&
            catalogoValue is CatalogoMaster catalogo
        )
        {
            Catalogo = catalogo;
        }

        ActualizarPropiedades();
    }

    [RelayCommand]
    private async Task EditarAsync()
    {
        if (Detalle is null)
            return;

        var parametros =
            new Dictionary<string, object>
            {
                ["Detalle"] = Detalle
            };

        if (Catalogo is not null)
        {
            parametros["Catalogo"] =
                Catalogo;
        }

        await Shell.Current.GoToAsync(
            nameof(CatalogoFormPage),
            parametros
        );
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (Detalle is null)
            return;

        var confirmar =
            await _confirmDialogService.ShowAsync(
                "Eliminar registro",
                $"¿Deseas eliminar \"{Detalle.Valor}\"?",
                "Eliminar",
                "Cancelar",
                "!"
            );

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var eliminado =
                await _catalogosService
                    .EliminarDetalleAsync(
                        Detalle.Id,
                        Detalle.IdCatalogo
                    );

            if (!eliminado)
            {
                ErrorMessage =
                    "No se pudo eliminar el registro.";

                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    ErrorMessage,
                    "Aceptar"
                );

                return;
            }

            await Shell.Current.GoToAsync("../..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            await Shell.Current.DisplayAlertAsync(
                "Error",
                "Ocurrió un error al eliminar.",
                "Aceptar"
            );
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

    private void ActualizarPropiedades()
    {
        OnPropertyChanged(nameof(EstadoTexto));
        OnPropertyChanged(nameof(EstadoColor));
        OnPropertyChanged(nameof(TieneDescripcion));
        OnPropertyChanged(nameof(TieneOrden));
        OnPropertyChanged(nameof(TieneValorExtra));
    }
}
