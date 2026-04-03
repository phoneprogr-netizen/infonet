using AppInfonet.Models;

namespace AppInfonet.Pages;

public partial class DispositivoUscitePage : ContentPage
{
    public DispositivoUscitePage(DashboardDeviceItem device)
    {
        InitializeComponent();
        BindingContext = device;
    }
}
