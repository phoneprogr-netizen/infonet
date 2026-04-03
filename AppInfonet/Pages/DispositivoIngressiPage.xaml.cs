using AppInfonet.Models;

namespace AppInfonet.Pages;

public partial class DispositivoIngressiPage : ContentPage
{
    public DispositivoIngressiPage(DashboardDeviceItem device)
    {
        InitializeComponent();
        BindingContext = device;
    }
}
