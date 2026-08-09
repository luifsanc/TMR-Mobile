using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class CargaActividadesViewModel : BaseViewModel
{
    private readonly ICargaActividadesService _cargaActividadesService;

    public ObservableCollection<ActividadListItem> Actividades { get; } = new();

    [ObservableProperty]
    public partial string ArchivoSeleccionado { get; set; } = "Ningún archivo seleccionado";

    [ObservableProperty]
    public partial bool TieneArchivoSeleccionado { get; set; }

    [ObservableProperty]
    public partial string ResultadoMensaje { get; set; } = string.Empty;

    private FileResult? _archivoElegido;

    public CargaActividadesViewModel(ICargaActividadesService cargaActividadesService)
    {
        _cargaActividadesService = cargaActividadesService;
        Title = "Carga de Actividades";
        _ = CargarActividadesAsync();
    }

   private async Task CargarActividadesAsync()
{
    IsBusy = true;
    try
    {
        var lista = await _cargaActividadesService.ObtenerActividadesAsync();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Actividades.Clear();
            foreach (var a in lista)
                Actividades.Add(a);
        });
    }
    finally
    {
        IsBusy = false;
    }
}

    [RelayCommand]
    private async Task SeleccionarArchivoAsync()
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "com.microsoft.excel.xls", "org.openxmlformats.spreadsheetml.sheet" } },
                { DevicePlatform.Android, new[] { "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                { DevicePlatform.WinUI, new[] { ".xlsx", ".xls" } },
            });

        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Seleccionar Excel de Actividades",
            FileTypes = customFileType
        });

        if (result != null)
        {
            _archivoElegido = result;
            ArchivoSeleccionado = result.FileName;
            TieneArchivoSeleccionado = true;
            ResultadoMensaje = string.Empty;
        }
    }

    [RelayCommand]
    private async Task ProcesarArchivoAsync()
    {
        if (_archivoElegido == null) return;

        IsBusy = true;
        ResultadoMensaje = string.Empty;

        try
        {
            using var stream = await _archivoElegido.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var resultado = await _cargaActividadesService.SubirExcelAsync(fileBytes, _archivoElegido.FileName);

            if (resultado.IsSuccess)
            {
                ResultadoMensaje = $"{resultado.Message} ({resultado.RegistrosProcesados} registros procesados)";
                ArchivoSeleccionado = "Ningún archivo seleccionado";
                TieneArchivoSeleccionado = false;
                _archivoElegido = null;
                await CargarActividadesAsync();
            }
            else
            {
                ResultadoMensaje = resultado.ErroresValidacion.Count > 0
                    ? $"{resultado.Message}: {string.Join(", ", resultado.ErroresValidacion)}"
                    : resultado.Message;
            }
        }
        catch (Exception ex)
        {
            ResultadoMensaje = $"Error al procesar el archivo: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}