using AppInfonet.Models;
using AppInfonet.Services;
using System.Collections.ObjectModel;

namespace AppInfonet.Pages;

public partial class AllarmiPage : ContentPage
{
    private readonly AuthApi _authApi;
    private readonly SessionStore _sessionStore;

    public ObservableCollection<AlarmEvent> Allarmi { get; } = new();

    public AllarmiPage(AuthApi authApi, SessionStore sessionStore)
    {
        InitializeComponent();
        _authApi = authApi;
        _sessionStore = sessionStore;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllarmiAsync();
    }

    private async Task LoadAllarmiAsync()
    {
        try
        {
            var items = await _authApi.GetAllarmiAsync();
            Allarmi.Clear();

            foreach (var item in items)
                Allarmi.Add(item);
        }
        catch
        {
            await DisplayAlert("Errore", "Impossibile caricare gli allarmi.", "OK");
        }
    }

    private async void OnStoricoClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Storico allarmi", "Sezione storico in arrivo.", "OK");
    }

    private async void OnApriMappaClicked(object sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: AlarmEvent allarme })
            return;

        if (!allarme.Latitudine.HasValue || !allarme.Longitudine.HasValue)
        {
            await DisplayAlert("Mappa", "Coordinate non disponibili per questo allarme.", "OK");
            return;
        }

        try
        {
            await Map.OpenAsync(allarme.Latitudine.Value, allarme.Longitudine.Value, new MapLaunchOptions
            {
                Name = allarme.MessaggioDisplay,
                NavigationMode = NavigationMode.None
            });
        }
        catch
        {
            var url = $"https://www.google.com/maps/search/?api=1&query={allarme.Latitudine.Value},{allarme.Longitudine.Value}";
            await Launcher.OpenAsync(url);
        }
    }

    private async void OnCloseEventClicked(object sender, EventArgs e)
    {
        await ExecuteEventActionAsync(sender, _authApi.CloseEventAsync, "Chiusura allarme");
    }

    private async void OnSnoozeEventClicked(object sender, EventArgs e)
    {
        var userId = _sessionStore.UserId ?? 0;
        await ExecuteEventActionAsync(
            sender,
            eventId => _authApi.SnoozeEventAsync(eventId, userId),
            "Snooze allarme");
    }

    private async void OnSendToOperationCenterClicked(object sender, EventArgs e)
    {
        var userId = _sessionStore.UserId ?? 0;
        await ExecuteEventActionAsync(
            sender,
            eventId => _authApi.SendToOperationCenterAsync(eventId, userId),
            "Invio a centrale operativa");
    }

    private async Task ExecuteEventActionAsync(
        object sender,
        Func<int, Task<(bool Success, string Message)>> action,
        string actionName)
    {
        if (sender is not Button { BindingContext: AlarmEvent allarme })
            return;

        var (success, message) = await action(allarme.IdTestataAllarme);
        if (!success)
        {
            await DisplayAlert(actionName, message, "OK");
            return;
        }

        await DisplayAlert(actionName, string.IsNullOrWhiteSpace(message) ? "Operazione completata." : message, "OK");
        await LoadAllarmiAsync();
    }
}
