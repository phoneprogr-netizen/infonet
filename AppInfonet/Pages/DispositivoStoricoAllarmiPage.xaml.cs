using AppInfonet.Models;
using System.Collections.ObjectModel;

namespace AppInfonet.Pages;

public partial class DispositivoStoricoAllarmiPage : ContentPage
{
    public ObservableCollection<StoricoAllarmeItem> StoricoAllarmi { get; } = new();

    public string DeviceName { get; }

    public DispositivoStoricoAllarmiPage(DashboardDeviceItem device)
    {
        InitializeComponent();

        DeviceName = string.IsNullOrWhiteSpace(device.DisplayName)
            ? device.COD_PERIF
            : device.DisplayName!;

        BindingContext = this;
    }

    public sealed class StoricoAllarmeItem
    {
        public string Titolo { get; init; } = string.Empty;

        public string DataOra { get; init; } = string.Empty;

        public string Stato { get; init; } = string.Empty;
    }
}
