using AppInfonet.Models;

namespace AppInfonet.Pages;


public partial class DashboardPage : ContentPage
{
	public DashboardPage()
	{
		InitializeComponent();
        BindingContext = new DashboardViewModel(); // se usi il ViewModel
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        if (services != null)
        {
            var profilePage = services.GetRequiredService<ProfiloPage>(); // o ProfiloPage
            await Navigation.PushAsync(profilePage);
        }
    }

    private async void OnDispositiviTapped(object sender, EventArgs e)
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        if (services != null)
        {
            var DispositiviPage = services.GetRequiredService<DispositiviPage>(); // o ProfiloPage
            await Navigation.PushAsync(DispositiviPage);
        }

    }

    private async void OnAllarmiTapped(object sender, EventArgs e)
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        if (services != null)
        {
            var allarmiPage = services.GetRequiredService<AllarmiPage>();
            await Navigation.PushAsync(allarmiPage);
        }
    }

    private async void OnBersagliTapped(object sender, EventArgs e)
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        if (services != null)
        {
            var BersagliPage = services.GetRequiredService<BersagliPage>(); // o ProfiloPage
            await Navigation.PushAsync(BersagliPage);
        }
    }

    private async void OnTragittiTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TragittiPage());
    }


}
