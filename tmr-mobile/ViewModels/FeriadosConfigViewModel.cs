using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Configuracion;
using tmr_mobile.Services;
using tmr_mobile.Views.Configuracion;

namespace tmr_mobile.ViewModels;

public partial class FeriadosConfigViewModel : BaseViewModel
{
    private readonly IFeriadosService _feriadosService;
    private static readonly CultureInfo CulturaEs = new("es-ES");

    public ObservableCollection<FeriadoItem> TodosFeriados { get; } = new();
    public ObservableCollection<FeriadoItem> FeriadosDelMes { get; } = new();
    public ObservableCollection<DiaCalendarioItem> DiasCalendario { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MesActualTexto))]
    public partial DateTime MesActual { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    public partial int TotalFeriados { get; set; }

    [ObservableProperty]
    public partial int TotalActivos { get; set; }

    [ObservableProperty]
    public partial int TotalNacionales { get; set; }

    [ObservableProperty]
    public partial int TotalLocales { get; set; }

    [ObservableProperty]
    public partial int TotalReligiosos { get; set; }

    public string MesActualTexto => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MesActual.ToString("MMMM yyyy", CulturaEs));

    public FeriadosConfigViewModel(IFeriadosService feriadosService)
    {
        _feriadosService = feriadosService;
        Title = "Configuración - Días Festivos";
    }

    public async Task InicializarAsync()
    {
        await CargarFeriadosAsync();
    }

    [RelayCommand]
    private async Task CargarFeriadosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var lista = await _feriadosService.ObtenerFeriadosAsync();

            TodosFeriados.Clear();
            foreach (var f in lista)
            {
                TodosFeriados.Add(f);
            }

            CalcularEstadisticas();
            GenerarCalendario();
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar los días festivos.";
            System.Diagnostics.Debug.WriteLine($"[FERIADOS][ERROR] {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CalcularEstadisticas()
    {
        TotalFeriados = TodosFeriados.Count;
        TotalActivos = TodosFeriados.Count(f => f.Activo);
        TotalNacionales = TodosFeriados.Count(f => string.Equals(f.TipoFeriado, "Nacional", StringComparison.OrdinalIgnoreCase));
        TotalLocales = TodosFeriados.Count(f => string.Equals(f.TipoFeriado, "Local", StringComparison.OrdinalIgnoreCase));
        TotalReligiosos = TodosFeriados.Count(f => string.Equals(f.TipoFeriado, "Religioso", StringComparison.OrdinalIgnoreCase));
    }

    private void GenerarCalendario()
    {
        DiasCalendario.Clear();
        FeriadosDelMes.Clear();

        var primerDiaMes = new DateTime(MesActual.Year, MesActual.Month, 1);
        var diasEnMes = DateTime.DaysInMonth(MesActual.Year, MesActual.Month);

        // Ajustar Lunes = 0 ... Domingo = 6
        int offsetInicio = ((int)primerDiaMes.DayOfWeek + 6) % 7;

        var inicioMatriz = primerDiaMes.AddDays(-offsetInicio);

        // Obtener feriados correspondientes al mes
        var feriadosDelMesLista = TodosFeriados.Where(f =>
            f.FechaFeriado.Month == MesActual.Month &&
            (f.EsRecurrente || f.FechaFeriado.Year == MesActual.Year)
        ).ToList();

        foreach (var fer in feriadosDelMesLista)
        {
            FeriadosDelMes.Add(fer);
        }

        for (int i = 0; i < 42; i++)
        {
            var fecha = inicioMatriz.AddDays(i);
            bool esMesActual = fecha.Month == MesActual.Month && fecha.Year == MesActual.Year;

            var feriadosDelDia = TodosFeriados.Where(f =>
                f.FechaFeriado.Day == fecha.Day &&
                f.FechaFeriado.Month == fecha.Month &&
                (f.EsRecurrente || f.FechaFeriado.Year == fecha.Year)
            ).ToList();

            DiasCalendario.Add(new DiaCalendarioItem
            {
                NumeroDia = fecha.Day,
                Fecha = fecha,
                EsMesActual = esMesActual,
                EsHoy = fecha.Date == DateTime.Today,
                Feriados = feriadosDelDia
            });
        }

        OnPropertyChanged(nameof(MesActualTexto));
    }

    [RelayCommand]
    private void NavegarMesAnterior()
    {
        MesActual = MesActual.AddMonths(-1);
        GenerarCalendario();
    }

    [RelayCommand]
    private void NavegarMesSiguiente()
    {
        MesActual = MesActual.AddMonths(1);
        GenerarCalendario();
    }

    [RelayCommand]
    private void IrAHoy()
    {
        MesActual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        GenerarCalendario();
    }

    [RelayCommand]
    private async Task NuevoFeriadoAsync()
    {
        await Shell.Current.GoToAsync(nameof(FeriadoFormPage));
    }

    [RelayCommand]
    private async Task AbrirDetalleAsync(FeriadoItem feriado)
    {
        if (feriado is null) return;

        var parametros = new Dictionary<string, object>
        {
            ["IdFeriado"] = feriado.Id,
            ["Feriado"] = feriado
        };

        await Shell.Current.GoToAsync(nameof(FeriadoDetallePage), parametros);
    }

    [RelayCommand]
    private async Task AbrirOpcionesFeriadoAsync(FeriadoItem feriado)
    {
        if (feriado is null) return;

        var accion = await Shell.Current.DisplayActionSheetAsync(
            feriado.NombreFeriado,
            "Cancelar",
            null,
            "👁 Ver más",
            "✏️ Editar",
            "🚫 Eliminar");

        if (accion == "👁 Ver más")
        {
            await AbrirDetalleAsync(feriado);
        }
        else if (accion == "✏️ Editar")
        {
            var parametros = new Dictionary<string, object>
            {
                ["IdFeriado"] = feriado.Id,
                ["Feriado"] = feriado
            };
            await Shell.Current.GoToAsync(nameof(FeriadoFormPage), parametros);
        }
        else if (accion == "🚫 Eliminar")
        {
            var confirm = await Shell.Current.DisplayAlertAsync("Confirmar eliminación", $"¿Deseas eliminar el feriado '{feriado.NombreFeriado}'?", "Sí", "No");
            if (confirm)
            {
                var success = await _feriadosService.EliminarFeriadoAsync(feriado.Id);
                if (success)
                {
                    await CargarFeriadosAsync();
                }
            }
        }
    }
}
