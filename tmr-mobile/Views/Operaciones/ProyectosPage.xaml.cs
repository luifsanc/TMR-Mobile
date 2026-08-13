using tmr_mobile.Models.Operaciones;
using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ProyectosPage : ContentPage
{
    public ProyectosPage(ProyectosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProyectosViewModel vm)
        {
            vm.CargarProyectosCommand.Execute(null);
        }
    }

    private async void OnProjectOptionsClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not ProyectosViewModel vm) return;
        if (sender is Button btn && btn.CommandParameter is ProyectoItem item)
        {
            var action = await DisplayActionSheetAsync("Opciones", "Cancelar", null, "Editar", "Inactivar");
            if (action == "Editar")
            {
                vm.AbrirEditar(item);
            }
            else if (action == "Inactivar")
            {
                var confirmar = await DisplayAlertAsync("Inactivar proyecto", $"¿Deseas inactivar el proyecto '{item.Nombre}'?", "Sí", "No");
                if (confirmar)
                {
                    await vm.InactivarProyectoAsync(item);
                }
            }
        }
    }

    private async void OnProjectDetailsClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not ProyectosViewModel vm) return;
        if (sender is Button btn && btn.CommandParameter is ProyectoItem item)
        {
            // Abre los detalles del proyecto o edición
            vm.AbrirEditar(item);
        }
    }
}
