using AppInfonet.Models;

namespace AppInfonet.Pages;

public class DispositivoIngressiUscitePage : TabbedPage
{
    public DispositivoIngressiUscitePage(DashboardDeviceItem device)
    {
        Title = "Ingressi / Uscite";

        Children.Add(new DispositivoIngressiPage(device)
        {
            Title = "Ingressi"
        });

        Children.Add(new DispositivoUscitePage(device)
        {
            Title = "Uscite"
        });
    }
}
