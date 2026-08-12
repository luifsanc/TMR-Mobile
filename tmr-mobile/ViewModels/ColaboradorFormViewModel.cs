using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class ColaboradorFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IColaboradoresService _colaboradoresService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    public partial int? IdColaborador { get; set; }

    public bool EsEdicion => IdColaborador.HasValue && IdColaborador.Value > 0;

    [ObservableProperty] public partial string TituloPagina { get; set; } = "Agregar Colaborador";

    // ── 1. Contrato ──
    [ObservableProperty] public partial ColaboradorCatalogoItem? EmpresaSeleccionada { get; set; }
    [ObservableProperty] public partial ColaboradorCatalogoItem? TipoContratoSeleccionado { get; set; }

    // ── 2. Datos personales ──
    [ObservableProperty] public partial string TipoPersona { get; set; } = "NATURAL";
    [ObservableProperty] public partial ColaboradorCatalogoItem? TipoIdentificacionSeleccionado { get; set; }
    [ObservableProperty] public partial string NumeroIdentificacion { get; set; } = string.Empty;
    [ObservableProperty] public partial string Nombres { get; set; } = string.Empty;
    [ObservableProperty] public partial string Apellidos { get; set; } = string.Empty;
    [ObservableProperty] public partial DateTime? FechaNacimiento { get; set; }
    [ObservableProperty] public partial ColaboradorCatalogoItem? GeneroSeleccionado { get; set; }
    [ObservableProperty] public partial ColaboradorCatalogoItem? NacionalidadSeleccionada { get; set; }

    // ── 3. Datos de contacto ──
    [ObservableProperty] public partial string Email { get; set; } = string.Empty;
    [ObservableProperty] public partial string Telefono { get; set; } = string.Empty;
    [ObservableProperty] public partial string Direccion { get; set; } = string.Empty;

    // ── 4. Datos laborales ──
    [ObservableProperty] public partial ColaboradorCatalogoItem? DepartamentoSeleccionado { get; set; }
    [ObservableProperty] public partial CargoItem? CargoSeleccionado { get; set; }
    [ObservableProperty] public partial DateTime FechaContratacion { get; set; } = DateTime.Today;
    [ObservableProperty] public partial int? AniosExperiencia { get; set; }
    [ObservableProperty] public partial ColaboradorCatalogoItem? ModalidadSeleccionada { get; set; }
    [ObservableProperty] public partial ColaboradorCatalogoItem? CategoriaSeleccionada { get; set; }
    [ObservableProperty] public partial ColaboradorModel? ReemplazoSeleccionado { get; set; }

    // Catálogos
    public ObservableCollection<ColaboradorCatalogoItem> Empresas { get; } = new();
    public ObservableCollection<ColaboradorCatalogoItem> TiposContrato { get; } = new();
    public ObservableCollection<ColaboradorCatalogoItem> TiposIdentificacion { get; } = new();
    public ObservableCollection<ColaboradorCatalogoItem> Departamentos { get; } = new();
    public ObservableCollection<CargoItem> Cargos { get; } = new();
    public ObservableCollection<ColaboradorCatalogoItem> Modalidades { get; } = new();
    public ObservableCollection<ColaboradorCatalogoItem> Categorias { get; } = new();
    public ObservableCollection<ColaboradorCatalogoItem> Generos { get; } = new();
    public ObservableCollection<ColaboradorCatalogoItem> Nacionalidades { get; } = new();
    public ObservableCollection<ColaboradorModel> ColaboradoresReemplazo { get; } = new();

    public ColaboradorFormViewModel(IColaboradoresService colaboradoresService)
    {
        _colaboradoresService = colaboradoresService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdColaborador", out var idObj) && idObj is int id)
        {
            IdColaborador = id;
            TituloPagina = "Editar Colaborador";
        }

        _ = CargarDatosInicialesAsync();
    }

    private async Task CargarDatosInicialesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Cargar catálogos en paralelo o secuencia
            var empresas = await _colaboradoresService.ObtenerCatalogoAsync("CAT");
            Empresas.Clear();
            foreach (var item in empresas) Empresas.Add(item);

            var contratos = await _colaboradoresService.ObtenerCatalogoAsync("TCT");
            TiposContrato.Clear();
            foreach (var item in contratos) TiposContrato.Add(item);

            var tids = await _colaboradoresService.ObtenerCatalogoAsync("TID");
            TiposIdentificacion.Clear();
            foreach (var item in tids) TiposIdentificacion.Add(item);

            var deps = await _colaboradoresService.ObtenerCatalogoAsync("DEP");
            Departamentos.Clear();
            foreach (var item in deps) Departamentos.Add(item);

            var mdts = await _colaboradoresService.ObtenerCatalogoAsync("MDT");
            Modalidades.Clear();
            foreach (var item in mdts) Modalidades.Add(item);

            var emps = await _colaboradoresService.ObtenerCatalogoAsync("EMP");
            Categorias.Clear();
            foreach (var item in emps) Categorias.Add(item);

            var gens = await _colaboradoresService.ObtenerCatalogoAsync("GEN");
            Generos.Clear();
            foreach (var item in gens) Generos.Add(item);

            var mdns = await _colaboradoresService.ObtenerCatalogoAsync("MDN");
            Nacionalidades.Clear();
            foreach (var item in mdns) Nacionalidades.Add(item);

            var listaReemplazo = await _colaboradoresService.ObtenerColaboradoresAsync(activo: true);
            ColaboradoresReemplazo.Clear();
            foreach (var item in listaReemplazo) ColaboradoresReemplazo.Add(item);

            // Si es edición, precargar los valores
            if (EsEdicion && IdColaborador.HasValue)
            {
                var c = await _colaboradoresService.ObtenerColaboradorAsync(IdColaborador.Value);
                if (c != null)
                {
                    TipoPersona = c.TipoPersona ?? "NATURAL";
                    NumeroIdentificacion = c.NumeroIdentificacion;
                    Nombres = c.Nombres;
                    Apellidos = c.Apellidos;
                    Email = c.Email;
                    Telefono = c.Telefono;
                    Direccion = c.Direccion;
                    AniosExperiencia = c.AniosExperiencia;

                    if (DateTime.TryParse(c.FechaNacimiento, out var fn)) FechaNacimiento = fn;
                    if (DateTime.TryParse(c.FechaContratacion ?? c.FechaIngreso, out var fc)) FechaContratacion = fc;

                    EmpresaSeleccionada = Empresas.FirstOrDefault(x => x.Id == c.IdEmpresaCatalogo);
                    TipoContratoSeleccionado = TiposContrato.FirstOrDefault(x => x.Id == c.IdTipoContrato);
                    TipoIdentificacionSeleccionado = TiposIdentificacion.FirstOrDefault(x => x.Id == c.IdTipoIdentificacion);
                    GeneroSeleccionado = Generos.FirstOrDefault(x => x.Id == c.IdGenero);
                    NacionalidadSeleccionada = Nacionalidades.FirstOrDefault(x => x.Id == c.IdNacionalidad);
                    ModalidadSeleccionada = Modalidades.FirstOrDefault(x => x.Id == c.IdModoTrabajo);
                    CategoriaSeleccionada = Categorias.FirstOrDefault(x => x.Id == c.IdCategoriaEmpleado);

                    DepartamentoSeleccionado = Departamentos.FirstOrDefault(x => x.Id == c.IdDepartamento);
                    if (DepartamentoSeleccionado != null)
                    {
                        await CargarCargosPorDepartamentoAsync(DepartamentoSeleccionado.Id);
                        CargoSeleccionado = Cargos.FirstOrDefault(x => x.Id == c.IdCargo);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al cargar datos iniciales.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnDepartamentoSeleccionadoChanged(ColaboradorCatalogoItem? value)
    {
        if (value != null)
        {
            _ = CargarCargosPorDepartamentoAsync(value.Id);
        }
        else
        {
            Cargos.Clear();
            CargoSeleccionado = null;
        }
    }

    private async Task CargarCargosPorDepartamentoAsync(int idDep)
    {
        var cargos = await _colaboradoresService.ObtenerCargosPorDepartamentoAsync(idDep);
        Cargos.Clear();
        foreach (var c in cargos) Cargos.Add(c);
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (!ValidarFormulario()) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (EsEdicion)
            {
                var request = new UpdateColaboradorRequest
                {
                    TipoPersona = TipoPersona,
                    NumeroIdentificacion = NumeroIdentificacion.Trim(),
                    IdTipoIdentificacion = TipoIdentificacionSeleccionado!.Id,
                    Nombres = Nombres.Trim(),
                    Apellidos = Apellidos.Trim(),
                    Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim(),
                    IdEmpresaCatalogo = EmpresaSeleccionada!.Id,
                    IdTipoContrato = TipoContratoSeleccionado!.Id,
                    IdDepartamento = DepartamentoSeleccionado!.Id,
                    IdCargo = CargoSeleccionado!.Id,
                    IdModoTrabajo = ModalidadSeleccionada?.Id,
                    IdCategoriaEmpleado = CategoriaSeleccionada?.Id,
                    AniosExperiencia = AniosExperiencia,
                    FechaContratacion = FechaContratacion.ToString("yyyy-MM-dd"),
                    FechaNacimiento = FechaNacimiento?.ToString("yyyy-MM-dd"),
                    IdGenero = GeneroSeleccionado?.Id,
                    IdNacionalidad = NacionalidadSeleccionada?.Id,
                    IdEmpleadoReemplazo = ReemplazoSeleccionado?.Id,
                    Activo = true
                };

                var exito = await _colaboradoresService.ActualizarColaboradorAsync(IdColaborador!.Value, request);
                if (exito) await Shell.Current.GoToAsync("..");
            }
            else
            {
                var request = new CreateColaboradorRequest
                {
                    TipoPersona = TipoPersona,
                    NumeroIdentificacion = NumeroIdentificacion.Trim(),
                    IdTipoIdentificacion = TipoIdentificacionSeleccionado!.Id,
                    Nombres = Nombres.Trim(),
                    Apellidos = Apellidos.Trim(),
                    Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim(),
                    IdEmpresaCatalogo = EmpresaSeleccionada!.Id,
                    IdTipoContrato = TipoContratoSeleccionado!.Id,
                    IdDepartamento = DepartamentoSeleccionado!.Id,
                    IdCargo = CargoSeleccionado!.Id,
                    IdModoTrabajo = ModalidadSeleccionada?.Id,
                    IdCategoriaEmpleado = CategoriaSeleccionada?.Id,
                    AniosExperiencia = AniosExperiencia,
                    FechaContratacion = FechaContratacion.ToString("yyyy-MM-dd"),
                    FechaNacimiento = FechaNacimiento?.ToString("yyyy-MM-dd"),
                    IdGenero = GeneroSeleccionado?.Id,
                    IdNacionalidad = NacionalidadSeleccionada?.Id,
                    IdEmpleadoReemplazo = ReemplazoSeleccionado?.Id
                };

                var nuevo = await _colaboradoresService.CrearColaboradorAsync(request);
                if (nuevo != null) await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar colaborador: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidarFormulario()
    {
        ErrorMessage = string.Empty;

        if (EmpresaSeleccionada == null) { ErrorMessage = "La empresa es requerida."; return false; }
        if (TipoContratoSeleccionado == null) { ErrorMessage = "El tipo de contrato es requerido."; return false; }
        if (TipoIdentificacionSeleccionado == null) { ErrorMessage = "El tipo de identificación es requerido."; return false; }
        if (string.IsNullOrWhiteSpace(NumeroIdentificacion)) { ErrorMessage = "La identificación es requerida."; return false; }
        if (string.IsNullOrWhiteSpace(Nombres)) { ErrorMessage = "Los nombres son requeridos."; return false; }
        if (string.IsNullOrWhiteSpace(Apellidos)) { ErrorMessage = "Los apellidos son requeridos."; return false; }
        if (DepartamentoSeleccionado == null) { ErrorMessage = "El departamento es requerido."; return false; }
        if (CargoSeleccionado == null) { ErrorMessage = "El cargo es requerido."; return false; }

        return true;
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
