using tmr_mobile.Services;
using tmr_shared.DTOs.Lideres;

namespace tmr_mobile.Views.Operaciones;

public partial class LiderFormPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly LiderResponse? _liderExistente;
    private List<TipoLiderResponse> _tiposLideres = new();
    private List<PersonaDisponibleResponse> _colaboradoresDisponibles = new();
    private PersonaDisponibleResponse? _colaboradorSeleccionado;
    private TipoLiderResponse? _tipoSeleccionado;
    private bool _cargandoCatalogos = false;

    public bool GuardadoExitoso { get; private set; }

    public LiderFormPage(ApiService apiService, LiderResponse? liderParaEditar = null)
    {
        InitializeComponent();
        _apiService = apiService;
        _liderExistente = liderParaEditar;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarCatalogosAsync();
    }

    private async Task CargarCatalogosAsync()
    {
        if (_cargandoCatalogos) return;
        _cargandoCatalogos = true;

        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        GuardarBtn.IsEnabled = false;

        try
        {
            // 1. Cargar tipos de líderes desde /api/lideres/tipos
            var tipos = await _apiService.GetAsync<List<TipoLiderResponse>>("lideres/tipos");
            _tiposLideres = tipos ?? new();
            TipoPicker.ItemsSource = _tiposLideres;

            // 2. Cargar colaboradores disponibles desde /api/lideres/personas-disponibles
            if (_liderExistente == null)
            {
                var colaboradores = await _apiService.GetAsync<List<PersonaDisponibleResponse>>("lideres/personas-disponibles");
                _colaboradoresDisponibles = colaboradores ?? new();
                ColaboradorPicker.ItemsSource = _colaboradoresDisponibles;
            }

            if (_liderExistente != null)
            {
                TitleLabel.Text = "Editar Líder";
                NombresEntry.Text = _liderExistente.Nombres;
                ApellidosEntry.Text = _liderExistente.Apellidos;
                EmailEntry.Text = _liderExistente.Email;
                TelefonoEntry.Text = _liderExistente.Telefono;

                // Seleccionar tipo existente
                _tipoSeleccionado = _tiposLideres.FirstOrDefault(t => t.Id == _liderExistente.Idtipo ||
                    t.Valor.Equals(_liderExistente.TipoBadge, StringComparison.OrdinalIgnoreCase) ||
                    t.Codigovalor.Equals(_liderExistente.Tipopersona, StringComparison.OrdinalIgnoreCase));

                if (_tipoSeleccionado != null)
                {
                    TipoPicker.SelectedItem = _tipoSeleccionado;
                }
                else if (_tiposLideres.Count > 0)
                {
                    TipoPicker.SelectedIndex = 0;
                }

                // En modo edición no se permite cambiar el tipo ni seleccionar colaborador
                TipoPicker.IsEnabled = false;
                ColaboradorContainer.IsVisible = false;

                // Si es interno, los nombres y apellidos son de solo lectura
                var esInterno = _liderExistente.TipoBadge == "Interno" || _liderExistente.Tipopersona == "I" || _tipoSeleccionado?.Codigovalor == "INT";
                NombresEntry.IsReadOnly = esInterno;
                ApellidosEntry.IsReadOnly = esInterno;
            }
            else
            {
                // Modo creación: seleccionar el primer tipo (usualmente Interno)
                if (_tiposLideres.Count > 0)
                {
                    TipoPicker.SelectedIndex = 0;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Error al cargar catálogos: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            GuardarBtn.IsEnabled = true;
            _cargandoCatalogos = false;
        }
    }

    private void OnTipoChanged(object? sender, EventArgs e)
    {
        _tipoSeleccionado = TipoPicker.SelectedItem as TipoLiderResponse;
        if (_tipoSeleccionado == null) return;

        var esInterno = _tipoSeleccionado.Codigovalor == "INT" ||
                        _tipoSeleccionado.Valor.Equals("Interno", StringComparison.OrdinalIgnoreCase);

        if (_liderExistente == null)
        {
            if (esInterno)
            {
                ColaboradorContainer.IsVisible = true;
                NombresEntry.IsReadOnly = true;
                ApellidosEntry.IsReadOnly = true;

                // Si ya había un colaborador seleccionado, rellenar
                if (_colaboradorSeleccionado != null)
                {
                    NombresEntry.Text = _colaboradorSeleccionado.Nombres;
                    ApellidosEntry.Text = _colaboradorSeleccionado.Apellidos;
                    EmailEntry.Text = _colaboradorSeleccionado.Email;
                    TelefonoEntry.Text = _colaboradorSeleccionado.Telefono;
                }
                else
                {
                    NombresEntry.Text = string.Empty;
                    ApellidosEntry.Text = string.Empty;
                    EmailEntry.Text = string.Empty;
                    TelefonoEntry.Text = string.Empty;
                }
            }
            else
            {
                // Externo: no necesita colaborador y permite escribir libremente
                ColaboradorContainer.IsVisible = false;
                ColaboradorPicker.SelectedItem = null;
                _colaboradorSeleccionado = null;
                NombresEntry.IsReadOnly = false;
                ApellidosEntry.IsReadOnly = false;
                NombresEntry.Text = string.Empty;
                ApellidosEntry.Text = string.Empty;
                EmailEntry.Text = string.Empty;
                TelefonoEntry.Text = string.Empty;
            }
        }
    }

    private void OnColaboradorChanged(object? sender, EventArgs e)
    {
        _colaboradorSeleccionado = ColaboradorPicker.SelectedItem as PersonaDisponibleResponse;
        if (_colaboradorSeleccionado != null)
        {
            NombresEntry.Text = _colaboradorSeleccionado.Nombres;
            ApellidosEntry.Text = _colaboradorSeleccionado.Apellidos;
            EmailEntry.Text = _colaboradorSeleccionado.Email;
            TelefonoEntry.Text = _colaboradorSeleccionado.Telefono;
            NombresEntry.IsReadOnly = true;
            ApellidosEntry.IsReadOnly = true;
            ErrorLabel.IsVisible = false;
        }
    }

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnGuardarClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        if (_tipoSeleccionado == null)
        {
            ErrorLabel.Text = "Por favor seleccione el tipo de líder.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var esInterno = _tipoSeleccionado.Codigovalor == "INT" ||
                        _tipoSeleccionado.Valor.Equals("Interno", StringComparison.OrdinalIgnoreCase);

        if (_liderExistente == null && esInterno && _colaboradorSeleccionado == null)
        {
            ErrorLabel.Text = "Debe seleccionar un colaborador para un líder interno.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var nombres = NombresEntry.Text?.Trim();
        var apellidos = ApellidosEntry.Text?.Trim();

        if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidos))
        {
            ErrorLabel.Text = "Los nombres y apellidos son requeridos.";
            ErrorLabel.IsVisible = true;
            return;
        }

        GuardarBtn.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            if (_liderExistente == null)
            {
                // Crear Líder (Compatible con el Backend)
                var nuevoReq = new CrearLiderRequest
                {
                    Idtipo = _tipoSeleccionado.Id,
                    Idpersona = esInterno ? _colaboradorSeleccionado?.Id : null,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Email = string.IsNullOrWhiteSpace(EmailEntry.Text) ? null : EmailEntry.Text.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(TelefonoEntry.Text) ? null : TelefonoEntry.Text.Trim(),
                    NumeroIdentificacion = null,
                    Usuariocreacion = "mobile",
                    Ipcreacion = "127.0.0.1"
                };

                var res = await _apiService.PostAsync<CrearLiderRequest, LiderResponse>("lideres", nuevoReq);
                GuardadoExitoso = res != null;
            }
            else
            {
                // Editar Líder
                var updateReq = new ActualizarLiderRequest
                {
                    Idtipo = _tipoSeleccionado.Id,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Email = string.IsNullOrWhiteSpace(EmailEntry.Text) ? null : EmailEntry.Text.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(TelefonoEntry.Text) ? null : TelefonoEntry.Text.Trim(),
                    NumeroIdentificacion = _liderExistente.NumeroIdentificacion,
                    Activo = _liderExistente.Activo,
                    Usuariomodificacion = "mobile",
                    Ipmodificacion = "127.0.0.1"
                };

                var ok = await _apiService.PutAsync($"lideres/{_liderExistente.Id}", updateReq);
                GuardadoExitoso = ok;
            }

            if (GuardadoExitoso)
            {
                if (_liderExistente == null)
                {
                    await DisplayAlert("Líder Creado", "El líder ha sido creado correctamente.", "Aceptar");
                }
                else
                {
                    await DisplayAlert("Líder Actualizado", "El líder ha sido actualizado correctamente.", "Aceptar");
                }

                await Navigation.PopModalAsync();
            }
            else
            {
                ErrorLabel.Text = "No se pudo guardar el líder. Verifique la información ingresada.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Error: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            GuardarBtn.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
