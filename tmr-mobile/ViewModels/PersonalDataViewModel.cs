using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public partial class PersonalDataViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IColaboradoresService _colaboradoresService;
    private bool _loaded;

    public PersonalDataViewModel(
        IAuthService authService,
        IColaboradoresService colaboradoresService)
    {
        _authService = authService;
        _colaboradoresService = colaboradoresService;
        Title = "Datos personales";
    }

    public string Name => _authService.CurrentUser?.Name ?? "Usuario TMR";
    public string Email => _authService.CurrentUser?.Email ?? "No disponible";
    public string EmployeeId => _authService.CurrentUser?.IdEmpleado?.ToString() ?? "No asignado";
    public string CreatedAt => _authService.CurrentUser is { } user
        ? user.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy")
        : "No disponible";

    [ObservableProperty] public partial string Phone { get; set; } = "No registrado";
    [ObservableProperty] public partial string JobTitle { get; set; } = "No registrado";

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_loaded || _authService.CurrentUser?.IdEmpleado is not int employeeId)
            return;

        try
        {
            IsBusy = true;
            var employee = await _colaboradoresService.ObtenerColaboradorAsync(employeeId);
            if (employee is null)
                return;

            Phone = string.IsNullOrWhiteSpace(employee.Telefono) ? "No registrado" : employee.Telefono;
            JobTitle = string.IsNullOrWhiteSpace(employee.Cargo) ? "No registrado" : employee.Cargo;
            _loaded = true;
        }
        catch
        {
            Phone = "No disponible";
            JobTitle = "No disponible";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
