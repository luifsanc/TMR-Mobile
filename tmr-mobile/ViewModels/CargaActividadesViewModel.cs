using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public partial class CargaActividadesViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _archivoSeleccionado = "Ningún archivo seleccionado";

    public CargaActividadesViewModel()
    {
        Title = "Carga Masiva de Actividades";
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
            ArchivoSeleccionado = result.FileName;
        }
    }
}
