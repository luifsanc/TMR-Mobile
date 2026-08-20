using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;
using tmr_mobile.Views.Operaciones;
using tmr_shared.DTOs.Proyectos;

namespace tmr_mobile.ViewModels;

public partial class ProyectosDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneProyecto))]
    public partial ProyectoResponse? Proyecto { get; set; }

    public bool TieneProyecto => Proyecto is not null;

    public ProyectosDetalleViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Detalle del Proyecto";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Proyecto", out var proyecto) && proyecto is ProyectoResponse response)
        {
            Proyecto = response;
        }
    }

    [RelayCommand]
    private async Task EditarAsync()
    {
        if (Proyecto is null) return;

        await Shell.Current.GoToAsync(nameof(ProyectosFormPage), new Dictionary<string, object>
        {
            ["IdProyecto"] = Proyecto.Id
        });
    }

    [RelayCommand]
    private async Task InactivarAsync()
    {
        if (Proyecto is null || IsBusy) return;

        var confirmar = await Shell.Current.CurrentPage.DisplayAlertAsync(
            "Inactivar proyecto",
            $"¿Deseas inactivar el proyecto '{Proyecto.Nombre}'?",
            "Sí", "No");

        if (!confirmar) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (await _apiService.DeleteAsync($"proyectos/{Proyecto.Id}"))
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = "No se pudo inactivar el proyecto.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al inactivar el proyecto.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
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
}
