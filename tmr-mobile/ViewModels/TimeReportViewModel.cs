using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public class ActividadItem
{
    public int Id { get; set; }
    public string Proyecto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public double Horas { get; set; }
    public DateTime Fecha { get; set; }
}

public partial class TimeReportViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ActividadItem> Actividades { get; } = new();

    public TimeReportViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Registro de Horas (Time Report)";
    }

    [RelayCommand]
    private async Task CargarActividadesAsync()
    {
        IsBusy = true;
        try
        {
            Actividades.Clear();
            Actividades.Add(new ActividadItem { Id = 1, Proyecto = "Sistema TMR", Descripcion = "Desarrollo de API REST .NET 10", Horas = 8.0, Fecha = DateTime.Now });
            Actividades.Add(new ActividadItem { Id = 2, Proyecto = "App Móvil MAUI", Descripcion = "Implementación de vistas en XAML", Horas = 6.5, Fecha = DateTime.Now.AddDays(-1) });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
