
using AppInfonet.Services;
using Microsoft.Maui.ApplicationModel;
using TuaApp.Utils;

namespace AppInfonet.Pages
{
    public partial class LoginPage : ContentPage
    {

        private readonly AuthApi _authApi;

        public LoginPage(AuthApi authApi)
        {
            InitializeComponent();
            _authApi = authApi;
            VersionLabel.Text = $"Versione {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";
        }

        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            ALog.Info("INFONET", "OnLoginClicked 1");
            ErrorLabel.IsVisible = false;

            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorLabel.Text = "Please enter username and password.";
                ErrorLabel.IsVisible = true;
                return;
            }

            ALog.Info("INFONET", "OnLoginClicked 2");

            try
            {
                var success = await _authApi.LoginAsync(username, password);
                if (!success)
                {
                    ErrorLabel.Text = "Invalid credentials.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                ALog.Info("INFONET", "OnLoginClicked 3");

                //await Navigation.PushAsync(new CommessePage());
                var services = App.Current?.Handler?.MauiContext?.Services;
                if (services != null)
                {
                    var dashboard = services.GetRequiredService<DashboardPage>();

                    // 🔥 IMPORTANTE: uso una NavigationPage
                    Application.Current.MainPage = new NavigationPage(dashboard);
                }
                else
                {
                    ErrorLabel.Text = "Errore interno: servizi non disponibili.";
                    ErrorLabel.IsVisible = true;
                }
                ALog.Info("INFONET", "OnLoginClicked 4");
            }
            catch (Exception ex)
            {
                ALog.Info("INFONET", "OnLoginClicked ERRORE"+ ex.Message  );
                ErrorLabel.Text = $"Login failed: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }

    }
}