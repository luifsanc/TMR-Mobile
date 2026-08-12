using tmr_mobile.Services;
using tmr_shared.DTOs.Lideres;

namespace tmr_mobile.Views.Operaciones;

public partial class LiderFormPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly LiderResponse? _liderExistente;
    public bool GuardadoExitoso { get; private set; }

    public LiderFormPage(ApiService apiService, LiderResponse? liderParaEditar = null)
    {
        InitializeComponent();
        _apiService = apiService;
        _liderExistente = liderParaEditar;

        if (_liderExistente != null)
        {
            TitleLabel.Text = "Editar Líder";
            NombresEntry.Text = _liderExistente.Nombres;
            ApellidosEntry.Text = _liderExistente.Apellidos;
            EmailEntry.Text = _liderExistente.Email;
            TelefonoEntry.Text = _liderExistente.Telefono;
            IdentificacionEntry.Text = _liderExistente.NumeroIdentificacion;
            TipoPicker.SelectedItem = _liderExistente.TipoBadge == "Externo" ? "Externo" : "Interno";
        }
        else
        {
            TipoPicker.SelectedIndex = 0;
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        var nombres = NombresEntry.Text?.Trim();
        var apellidos = ApellidosEntry.Text?.Trim();

        if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidos))
        {
            ErrorLabel.Text = "Los nombres y apellidos son requeridos.";
            ErrorLabel.IsVisible = true;
            return;
        }

        GuardarBtn.IsEnabled = false;
        ErrorLabel.IsVisible = false;

        try
        {
            var tipoPersona = TipoPicker.SelectedItem?.ToString() == "Externo" ? "E" : "I";

            if (_liderExistente == null)
            {
                // Crear Líder
                var nuevoReq = new CrearLiderRequest
                {
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Email = EmailEntry.Text?.Trim(),
                    Telefono = TelefonoEntry.Text?.Trim(),
                    Tipopersona = tipoPersona,
                    NumeroIdentificacion = IdentificacionEntry.Text?.Trim() ?? string.Empty
                };

                var res = await _apiService.PostAsync<CrearLiderRequest, LiderResponse>("api/lideres", nuevoReq);
                GuardadoExitoso = res != null;
            }
            else
            {
                // Editar Líder
                var updateReq = new ActualizarLiderRequest
                {
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Email = EmailEntry.Text?.Trim(),
                    Telefono = TelefonoEntry.Text?.Trim(),
                    Tipopersona = tipoPersona,
                    NumeroIdentificacion = IdentificacionEntry.Text?.Trim() ?? string.Empty,
                    Activo = _liderExistente.Activo
                };

                var ok = await _apiService.PutAsync($"api/lideres/{_liderExistente.Id}", updateReq);
                GuardadoExitoso = ok;
            }

            if (GuardadoExitoso)
            {
                await Navigation.PopModalAsync();
            }
            else
            {
                ErrorLabel.Text = "Error al guardar el líder en el backend.";
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
        }
    }
}
