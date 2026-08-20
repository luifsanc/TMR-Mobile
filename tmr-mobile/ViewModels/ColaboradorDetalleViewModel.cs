using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;
using tmr_mobile.Views.Operaciones;

namespace tmr_mobile.ViewModels;

public partial class ColaboradorDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IColaboradoresService _colaboradoresService;
    private int _idColaborador;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsActivo))]
    [NotifyPropertyChangedFor(nameof(EsInactivo))]
    [NotifyPropertyChangedFor(nameof(TieneColaborador))]
    [NotifyPropertyChangedFor(nameof(TieneProyectos))]
    [NotifyPropertyChangedFor(nameof(MostrarSalidaInfo))]
    public partial ColaboradorDetalleModel? Colaborador { get; set; }

    public bool EsActivo => Colaborador?.Activo == true;
    public bool EsInactivo => Colaborador != null && !Colaborador.Activo;
    public bool TieneColaborador => Colaborador != null;
    public bool TieneProyectos => Colaborador?.Proyectos.Any() == true;
    public bool MostrarSalidaInfo => Colaborador != null && !Colaborador.Activo && Colaborador.TieneSalidaInfo;

    public ColaboradorDetalleViewModel(IColaboradoresService colaboradoresService)
    {
        _colaboradoresService = colaboradoresService;
        Title = "Detalle del Colaborador";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdColaborador", out var idObj) && idObj is int id)
        {
            _idColaborador = id;
            _ = CargarDetalleAsync();
        }
    }

    [RelayCommand]
    private async Task CargarDetalleAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            Colaborador = await _colaboradoresService.ObtenerColaboradorAsync(_idColaborador);

            if (Colaborador == null)
            {
                ErrorMessage = "No se pudo cargar el detalle del colaborador.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al cargar los detalles.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditarAsync()
    {
        if (Colaborador == null) return;

        var parametros = new Dictionary<string, object>
        {
            ["IdColaborador"] = Colaborador.Id
        };

        await Shell.Current.GoToAsync(nameof(ColaboradorFormPage), parametros);
    }

    [RelayCommand]
    private async Task RegistrarSalidaAsync()
    {
        if (Colaborador == null) return;

        var parametros = new Dictionary<string, object>
        {
            ["IdColaborador"] = Colaborador.Id
        };

        await Shell.Current.GoToAsync(nameof(ColaboradorSalidaPage), parametros);
    }

    [RelayCommand]
    private async Task ActivarAsync()
    {
        if (Colaborador == null || IsBusy) return;

        bool confirmacion = await Shell.Current.CurrentPage.DisplayAlert(
            "Confirmación",
            "¿Estás seguro de reactivar a este colaborador?",
            "Sí", "No");

        if (!confirmacion) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var request = new UpdateColaboradorRequest
            {
                TipoPersona = Colaborador.TipoPersona ?? "NATURAL",
                NumeroIdentificacion = Colaborador.NumeroIdentificacion,
                IdTipoIdentificacion = Colaborador.IdTipoIdentificacion ?? 1,
                Nombres = Colaborador.Nombres,
                Apellidos = Colaborador.Apellidos,
                Email = Colaborador.Email,
                Telefono = Colaborador.Telefono,
                Direccion = Colaborador.Direccion,
                IdEmpresaCatalogo = Colaborador.IdEmpresaCatalogo ?? 1,
                IdTipoContrato = Colaborador.IdTipoContrato ?? 1,
                IdDepartamento = Colaborador.IdDepartamento ?? 1,
                IdCargo = Colaborador.IdCargo ?? 1,
                IdModoTrabajo = Colaborador.IdModoTrabajo,
                IdCategoriaEmpleado = Colaborador.IdCategoriaEmpleado,
                AniosExperiencia = Colaborador.AniosExperiencia,
                FechaContratacion = Colaborador.FechaContratacion,
                FechaNacimiento = Colaborador.FechaNacimiento,
                IdGenero = Colaborador.IdGenero,
                IdNacionalidad = Colaborador.IdNacionalidad,
                Activo = true // Re-activación
            };

            var exito = await _colaboradoresService.ActualizarColaboradorAsync(Colaborador.Id, request);
            if (exito)
            {
                await CargarDetalleAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.CurrentPage.DisplayAlert("Error", "No se pudo activar el colaborador.", "OK");
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
