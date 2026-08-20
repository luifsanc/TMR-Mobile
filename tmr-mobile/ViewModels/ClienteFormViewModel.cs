using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Models.Operaciones;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class ClienteFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IClientesService _clientesService;

    // Estado del formulario
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    public partial int? IdCliente { get; set; }

    public bool EsEdicion => IdCliente.HasValue && IdCliente.Value > 0;

    [ObservableProperty]
    public partial string TituloPagina { get; set; } = "Nuevo Cliente";

    // Campos del formulario
    [ObservableProperty] public partial TipoIdentificacionModel? TipoIdentificacionSeleccionado { get; set; }
    [ObservableProperty] public partial string NumeroIdentificacion { get; set; } = string.Empty;
    [ObservableProperty] public partial string NombreComercial { get; set; } = string.Empty;
    [ObservableProperty] public partial string Nombres { get; set; } = string.Empty;
    [ObservableProperty] public partial string Apellidos { get; set; } = string.Empty;
    [ObservableProperty] public partial string Email { get; set; } = string.Empty;
    [ObservableProperty] public partial string Telefono { get; set; } = string.Empty;
    [ObservableProperty] public partial string Direccion { get; set; } = string.Empty;
    
    // Sólo para edición
    [ObservableProperty] public partial bool Activo { get; set; } = true;

    // Catálogos
    public ObservableCollection<TipoIdentificacionModel> TiposIdentificacion { get; } = new();

    public ClienteFormViewModel(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdCliente", out var idObj) && idObj is int id)
        {
            IdCliente = id;
            TituloPagina = "Editar Cliente";
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

            // 1. Cargar tipos de identificación
            var tipos = await _clientesService.ObtenerTiposIdentificacionAsync();
            TiposIdentificacion.Clear();
            foreach (var t in tipos)
            {
                TiposIdentificacion.Add(t);
            }

            // 2. Si es edición, cargar datos del cliente
            if (EsEdicion && IdCliente.HasValue)
            {
                var cliente = await _clientesService.ObtenerClienteAsync(IdCliente.Value);
                if (cliente != null)
                {
                    NumeroIdentificacion = cliente.NumeroIdentificacion;
                    NombreComercial = cliente.NombreComercial;
                    Nombres = cliente.Nombres;
                    Apellidos = cliente.Apellidos;
                    Email = cliente.Email;
                    Telefono = cliente.Telefono;
                    Direccion = cliente.Direccion;
                    Activo = cliente.Activo;

                    // Seleccionar el tipo de id
                    TipoIdentificacionSeleccionado = TiposIdentificacion.FirstOrDefault(t => 
                        t.Valor.Equals(cliente.TipoIdentificacion, StringComparison.OrdinalIgnoreCase) ||
                        (cliente.TipoIdentificacion.Contains("ruc", StringComparison.OrdinalIgnoreCase) && t.Valor.Contains("ruc", StringComparison.OrdinalIgnoreCase)) ||
                        (cliente.TipoIdentificacion.Contains("ced", StringComparison.OrdinalIgnoreCase) && t.Valor.Contains("ced", StringComparison.OrdinalIgnoreCase))
                    );
                }
            }
            else
            {
                // Si es nuevo cliente, seleccionar el primer tipo de identificación por defecto
                if (TiposIdentificacion.Count > 0 && TipoIdentificacionSeleccionado == null)
                {
                    TipoIdentificacionSeleccionado = TiposIdentificacion.FirstOrDefault();
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al cargar los datos.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
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
                var request = new UpdateClienteRequest
                {
                    IdTipoIdentificacion = TipoIdentificacionSeleccionado!.Id,
                    NumeroIdentificacion = NumeroIdentificacion.Trim(),
                    NombreComercial = NombreComercial.Trim(),
                    Nombres = Nombres.Trim(),
                    Apellidos = Apellidos.Trim(),
                    Email = Email.Trim(),
                    Telefono = Telefono.Trim(),
                    Direccion = Direccion.Trim(),
                    Activo = Activo
                };

                var resultado = await _clientesService.ActualizarClienteAsync(IdCliente!.Value, request);
                if (resultado.Success)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = string.IsNullOrWhiteSpace(resultado.Message)
                        ? "Error al actualizar el cliente."
                        : resultado.Message;
                }
            }
            else
            {
                var request = new CreateClienteRequest
                {
                    IdTipoIdentificacion = TipoIdentificacionSeleccionado!.Id,
                    NumeroIdentificacion = NumeroIdentificacion.Trim(),
                    NombreComercial = NombreComercial.Trim(),
                    Nombres = Nombres.Trim(),
                    Apellidos = Apellidos.Trim(),
                    Email = Email.Trim(),
                    Telefono = Telefono.Trim(),
                    Direccion = Direccion.Trim()
                };

                var resultado = await _clientesService.CrearClienteAsync(request);
                if (resultado.Success)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ErrorMessage = string.IsNullOrWhiteSpace(resultado.Message)
                        ? "Error al crear el cliente."
                        : resultado.Message;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidarFormulario()
    {
        ErrorMessage = string.Empty;

        if (TipoIdentificacionSeleccionado == null)
        {
            ErrorMessage = "Debe seleccionar un tipo de identificación.";
            return false;
        }

        var numId = (NumeroIdentificacion ?? "").Trim();

        if (string.IsNullOrWhiteSpace(numId))
        {
            ErrorMessage = "El número de identificación es requerido.";
            return false;
        }
        
        if (numId.Length > 20)
        {
            ErrorMessage = "El número de identificación no puede superar los 20 caracteres.";
            return false;
        }

        var tipoTexto = (TipoIdentificacionSeleccionado.Valor ?? "").Trim().ToLowerInvariant();

        if (tipoTexto.Contains("ced") || tipoTexto.Contains("céd"))
        {
            if (numId.Length != 10 || !numId.All(char.IsDigit))
            {
                ErrorMessage = "La cédula debe contener exactamente 10 dígitos numéricos.";
                return false;
            }
        }
        else if (tipoTexto.Contains("ruc"))
        {
            if (numId.Length != 13 || !numId.All(char.IsDigit))
            {
                ErrorMessage = "El RUC debe contener exactamente 13 dígitos numéricos.";
                return false;
            }
        }
        else if (tipoTexto.Contains("pasaporte") || tipoTexto.Contains("otro"))
        {
            if (!numId.All(char.IsLetterOrDigit))
            {
                ErrorMessage = "El número de identificación debe ser alfanumérico.";
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(NombreComercial))
        {
            ErrorMessage = "El nombre comercial es requerido.";
            return false;
        }

        if (NombreComercial.Length > 100)
        {
            ErrorMessage = "El nombre comercial no puede superar los 100 caracteres.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Nombres))
        {
            ErrorMessage = "Los nombres son requeridos.";
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(Apellidos))
        {
            ErrorMessage = "Los apellidos son requeridos.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email) || !System.Text.RegularExpressions.Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ErrorMessage = "El correo electrónico es requerido y debe tener un formato válido (ej: usuario@dominio.com).";
            return false;
        }
        
        if (Email.Length > 100)
        {
            ErrorMessage = "El correo electrónico no puede superar los 100 caracteres.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Telefono))
        {
            ErrorMessage = "El teléfono es requerido.";
            return false;
        }
        
        if (Telefono.Length > 20)
        {
            ErrorMessage = "El teléfono no puede superar los 20 caracteres.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Direccion))
        {
            ErrorMessage = "La dirección es requerida.";
            return false;
        }
        
        if (Direccion.Length > 255)
        {
            ErrorMessage = "La dirección no puede superar los 255 caracteres.";
            return false;
        }

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
