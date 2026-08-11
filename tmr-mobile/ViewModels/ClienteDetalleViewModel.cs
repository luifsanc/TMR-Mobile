using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;
using tmr_mobile.Views.Operaciones;

namespace tmr_mobile.ViewModels;

public partial class ClienteDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IClientesService _clientesService;
    private int _idCliente;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsActivo))]
    [NotifyPropertyChangedFor(nameof(EsInactivo))]
    [NotifyPropertyChangedFor(nameof(TextoBotonEstado))]
    [NotifyPropertyChangedFor(nameof(TieneCliente))]
    public partial ClienteDetalleModel? Cliente { get; set; }

    public bool EsActivo => Cliente?.Activo == true;
    public bool EsInactivo => Cliente != null && !Cliente.Activo;
    
    public bool TieneCliente => Cliente != null;
    public bool TieneError => !string.IsNullOrEmpty(ErrorMessage);
    
    public string TextoBotonEstado => EsActivo ? "Desactivar" : "Activar";

    public ClienteDetalleViewModel(IClientesService clientesService)
    {
        _clientesService = clientesService;
        Title = "Detalle de Cliente";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdCliente", out var idObj) && idObj is int id)
        {
            _idCliente = id;
            // Se invoca asíncronamente sin bloquear
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
            
            Cliente = await _clientesService.ObtenerClienteAsync(_idCliente);
            
            if (Cliente == null)
            {
                ErrorMessage = "No se pudo cargar el cliente o no existe.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al cargar el detalle del cliente.";
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
        if (Cliente == null) return;
        
        var parametros = new Dictionary<string, object>
        {
            ["IdCliente"] = Cliente.Id
        };
        
        await Shell.Current.GoToAsync(nameof(ClienteFormPage), parametros);
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync()
    {
        if (Cliente == null || IsBusy) return;

        bool confirmacion = await Shell.Current.CurrentPage.DisplayAlert(
            "Confirmación",
            $"¿Estás seguro de {(EsActivo ? "desactivar" : "activar")} a este cliente?",
            "Sí", "No");

        if (!confirmacion) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var request = new UpdateClienteRequest
            {
                IdTipoIdentificacion = 0, // El backend podría necesitar esto o no en un cambio de estado, lo ideal es enviar todos los datos.
                NumeroIdentificacion = Cliente.NumeroIdentificacion,
                NombreComercial = Cliente.NombreComercial,
                Nombres = Cliente.Nombres,
                Apellidos = Cliente.Apellidos,
                Email = Cliente.Email,
                Telefono = Cliente.Telefono,
                Direccion = Cliente.Direccion,
                Activo = !Cliente.Activo // Invertimos el estado actual
            };

            // Aquí el ideal es que conozcamos el IdTipoIdentificacion.
            // Puesto que ClienteDetalleModel en el backend devuelve el string TipoIdentificacion,
            // tendremos que lidiar con enviar todos los datos. En un caso real, o el backend expone un PATCH,
            // o debemos cargar primero el TipoIdentificacionId, que en ClienteDetalleModel no lo tenemos si sólo trae string.
            // Para resolverlo robustamente, se podría crear un endpoint PATCH en el futuro.
            // Por ahora enviaremos 0, asumiendo que si no se modifica desde el combo, el backend o falla o acepta 0.
            // En un flujo real, si el backend valida IdTipoIdentificacion > 0 (como en ActualizarClienteRequestValidator),
            // la petición fallaría.
            // Como requerimiento, enviaremos la petición y si falla, mostraremos el error.
            // Si esto falla, el usuario tendrá que usar Editar y guardar desde ahí.
            // Pero intentémoslo:
            // NOTA: Como workaround para no fallar el validato "GreaterThan(0)", si el front/back no proveen el Id en Detalle,
            // no podemos enviar un PUT completo. El Angular frontend tiene un caché de tipos para obtener el IdTipoIdentificacion
            // basándose en el string "RUC", "Cédula", etc. Haré lo mismo.

            var tipos = await _clientesService.ObtenerTiposIdentificacionAsync();
            var tipoSeleccionado = tipos.FirstOrDefault(t => 
                t.Valor.Equals(Cliente.TipoIdentificacion, StringComparison.OrdinalIgnoreCase) ||
                (Cliente.TipoIdentificacion.Contains("ruc", StringComparison.OrdinalIgnoreCase) && t.Valor.Contains("ruc", StringComparison.OrdinalIgnoreCase)) ||
                (Cliente.TipoIdentificacion.Contains("ced", StringComparison.OrdinalIgnoreCase) && t.Valor.Contains("ced", StringComparison.OrdinalIgnoreCase))
            );

            request.IdTipoIdentificacion = tipoSeleccionado?.Id ?? 1; // Fallback al primero

            var exito = await _clientesService.ActualizarClienteAsync(Cliente.Id, request);
            
            if (exito)
            {
                // Recargar el detalle
                await CargarDetalleAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.CurrentPage.DisplayAlert("Error", "No se pudo cambiar el estado.", "OK");
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
