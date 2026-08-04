using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace tmr_mobile.ViewModels;

public class LiderItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
}

public partial class LideresViewModel : BaseViewModel
{
    public ObservableCollection<LiderItem> Lideres { get; } = new();

    public LideresViewModel()
    {
        Title = "Líderes de Proyecto";
    }

    [RelayCommand]
    private async Task CargarLideresAsync()
    {
        IsBusy = true;
        try
        {
            Lideres.Clear();
            Lideres.Add(new LiderItem { Id = 1, Nombre = "Ing. Roberto Gómez", Area = "Tecnología e Innovación" });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
