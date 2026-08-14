using tmr_mobile.Models.Operaciones;
using tmr_mobile.ViewModels;

namespace tmr_mobile.Views.Operaciones;

public partial class ProyectosPage : ContentPage
{
    private tmr_shared.DTOs.Proyectos.ProyectoResponse? _proyectoSeleccionado;

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

        if (SearchEntry != null)
        {
            SearchEntry.TextChanged += OnSearchTextChanged;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (SearchEntry != null)
        {
            SearchEntry.TextChanged -= OnSearchTextChanged;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is ProyectosViewModel vm)
        {
            vm.BuscarCommand.Execute(e.NewTextValue);
        }
    }

    private void OnMoreOptionsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is tmr_shared.DTOs.Proyectos.ProyectoResponse proyecto)
        {
            _proyectoSeleccionado = proyecto;
            ContextMenuOverlay.IsVisible = true;
        }
    }

    private void OnCloseMenuTapped(object sender, EventArgs e)
    {
        ContextMenuOverlay.IsVisible = false;
        _proyectoSeleccionado = null;
    }

    private void OnCloseProjectDetailTapped(object sender, EventArgs e)
    {
        ProjectDetailOverlay.IsVisible = false;
    }

    private void ShowProjectDetailModal(tmr_shared.DTOs.Proyectos.ProyectoResponse proyecto)
    {
        ProjectDetailName.Text = proyecto.Nombre;
        ProjectDetailClient.Text = proyecto.Cliente;
        ProjectDetailStatus.Text = proyecto.Estado;
        ProjectDetailLeader.Text = string.IsNullOrWhiteSpace(proyecto.LiderAsignado) ? "No asignado" : proyecto.LiderAsignado;
        ProjectDetailResources.Text = proyecto.NumeroRecursos.ToString();

        var fechaInicio = proyecto.FechaInicio.HasValue ? proyecto.FechaInicio.Value.ToString("dd/MM/yyyy") : "-";
        var fechaFin = proyecto.FechaFin.HasValue ? proyecto.FechaFin.Value.ToString("dd/MM/yyyy") : "-";
        ProjectDetailDates.Text = $"{fechaInicio} - {fechaFin}";

        if (proyecto.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase))
        {
            ProjectDetailStatusBadge.BackgroundColor = Color.FromArgb("#DCFCE7");
            ProjectDetailStatus.TextColor = Color.FromArgb("#166534");
        }
        else if (proyecto.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
        {
            ProjectDetailStatusBadge.BackgroundColor = Color.FromArgb("#FEE2E2");
            ProjectDetailStatus.TextColor = Color.FromArgb("#991B1B");
        }
        else
        {
            ProjectDetailStatusBadge.BackgroundColor = Color.FromArgb("#E0F2FE");
            ProjectDetailStatus.TextColor = Color.FromArgb("#0F766E");
        }

        ProjectDetailOverlay.IsVisible = true;
    }

    private void OnMenuViewClicked(object sender, EventArgs e)
    {
        if (_proyectoSeleccionado == null) return;

        ContextMenuOverlay.IsVisible = false;
        ShowProjectDetailModal(_proyectoSeleccionado);
    }

    private async void OnMenuEditClicked(object sender, EventArgs e)
    {
        if (_proyectoSeleccionado == null) return;

        var parametros = new Dictionary<string, object>
        {
            ["IdProyecto"] = _proyectoSeleccionado.Id
        };

        ContextMenuOverlay.IsVisible = false;
        await Shell.Current.GoToAsync(nameof(ProyectosFormPage), parametros);
    }

    private async void OnMenuDeactivateClicked(object sender, EventArgs e)
    {
        if (_proyectoSeleccionado == null) return;

        bool confirm = await DisplayAlert("Confirmación", $"¿Deseas inactivar el proyecto '{_proyectoSeleccionado.Nombre}'?", "Sí", "No");
        if (!confirm)
        {
            ContextMenuOverlay.IsVisible = false;
            return;
        }

        ContextMenuOverlay.IsVisible = false;

        if (BindingContext is ProyectosViewModel vm)
        {
            await vm.InactivarProyectoAsync(_proyectoSeleccionado.Id);
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
