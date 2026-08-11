using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using tmr_mobile.Services;

namespace tmr_mobile.ViewModels;

public class ColaboradorItem
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CodigoEmpleado { get; set; } = string.Empty;
    public int NumProyectos { get; set; }
}

public sealed class ColaboradorApiResponse
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CodigoEmpleado { get; set; } = string.Empty;
    public int NumProyectos { get; set; }
}

public partial class ColaboradoresViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ColaboradorItem> Colaboradores { get; } = new();

    public ColaboradoresViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Gestión de Colaboradores";
    }

    [RelayCommand]
    private async Task CargarColaboradoresAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var colaboradores = await _apiService.GetAsync<List<ColaboradorApiResponse>>("colaboradores");
            Colaboradores.Clear();

            foreach (var colaborador in colaboradores ?? new List<ColaboradorApiResponse>())
            {
                Colaboradores.Add(new ColaboradorItem
                {
                    Id = colaborador.Id,
                    NombreCompleto = colaborador.NombreCompleto,
                    Cargo = colaborador.Cargo,
                    Email = colaborador.Email,
                    CodigoEmpleado = colaborador.CodigoEmpleado,
                    NumProyectos = colaborador.NumProyectos
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar colaboradores: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
