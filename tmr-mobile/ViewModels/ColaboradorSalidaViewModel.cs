using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class ColaboradorSalidaViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IColaboradoresService _colaboradoresService;
    private int _idColaborador;

    [ObservableProperty] public partial ColaboradorDetalleModel? Colaborador { get; set; }

    [ObservableProperty] public partial DateTime FechaSalida { get; set; } = DateTime.Today;
    [ObservableProperty] public partial ColaboradorCatalogoItem? TipoSalidaSeleccionado { get; set; }
    [ObservableProperty] public partial ColaboradorCatalogoItem? CausaSalidaSeleccionada { get; set; }
    [ObservableProperty] public partial string Comentario { get; set; } = string.Empty;

    public ObservableCollection<ColaboradorCatalogoItem> TiposSalida { get; } = new();
    public ObservableCollection<ColaboradorCatalogoItem> CausasSalida { get; } = new();

    public ColaboradorSalidaViewModel(IColaboradoresService colaboradoresService)
    {
        _colaboradoresService = colaboradoresService;
        Title = "Registrar Salida";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdColaborador", out var idObj) && idObj is int id)
        {
            _idColaborador = id;
            _ = CargarDatosInicialesAsync();
        }
    }

    private async Task CargarDatosInicialesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // 1. Detalle del colaborador
            Colaborador = await _colaboradoresService.ObtenerColaboradorAsync(_idColaborador);

            // 2. Catálogos de salida
            var tipos = await _colaboradoresService.ObtenerCatalogoAsync("TOS");
            TiposSalida.Clear();
            foreach (var t in tipos) TiposSalida.Add(t);

            var causas = await _colaboradoresService.ObtenerCatalogoAsync("CAS");
            CausasSalida.Clear();
            foreach (var c in causas) CausasSalida.Add(c);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al cargar datos de salida.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GuardarSalidaAsync()
    {
        if (TipoSalidaSeleccionado == null)
        {
            ErrorMessage = "Debe seleccionar un tipo de salida.";
            return;
        }

        if (CausaSalidaSeleccionada == null)
        {
            ErrorMessage = "Debe seleccionar una causa de salida.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var request = new RegistrarSalidaRequest
            {
                FechaSalida = FechaSalida.ToString("yyyy-MM-dd"),
                IdTipoSalida = TipoSalidaSeleccionado.Id,
                IdCausaSalida = CausaSalidaSeleccionada.Id,
                Comentario = string.IsNullOrWhiteSpace(Comentario) ? null : Comentario.Trim()
            };

            var exito = await _colaboradoresService.RegistrarSalidaAsync(_idColaborador, request);
            if (exito)
            {
                // Regresar al detalle
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al registrar salida: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void LimpiarError()
    {
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task RegresarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
