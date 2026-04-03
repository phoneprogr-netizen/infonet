using AppInfonet.Models;
using AppInfonet.Services;
using System.Collections.ObjectModel;

namespace AppInfonet.Pages;

public partial class AllarmiPage : ContentPage
{
    private readonly AuthApi _authApi;
    private readonly SessionStore _sessionStore;

    public ObservableCollection<AlarmEvent> Allarmi { get; } = new();
    private bool _isLoading;
    private bool _isBusyLoadingAllarmi;

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged();
        }
    }

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
        if (_isBusyLoadingAllarmi)
            return;

        _isBusyLoadingAllarmi = true;
        IsLoading = true;

        try
        {
            List<AlarmEvent> items = new();
            Exception? lastException = null;

            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    items = await _authApi.GetAllarmiAsync();
                    lastException = null;
                    break;
                }
                catch (InvalidOperationException ex) when (attempt < 3)
                {
                    lastException = ex;
                    await Task.Delay(300);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    break;
                }
            }

            if (lastException is not null)
                throw lastException;

            Allarmi.Clear();

            foreach (var item in items)
                Allarmi.Add(item);
        }
        catch
        {
            await DisplayAlert("Errore", "Impossibile caricare gli allarmi.", "OK");
        }
        finally
        {
            IsLoading = false;
            _isBusyLoadingAllarmi = false;
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
        var confirmed = await ConfirmEventActionAsync(
            sender,
            "Chiusura allarme",
            "Sei sicuro di voler chiudere l'allarme?");

        if (!confirmed)
            return;

        await ExecuteEventActionAsync(sender, _authApi.CloseEventAsync, "Chiusura allarme");
    }

    private async void OnSnoozeEventClicked(object sender, EventArgs e)
    {
        var confirmed = await ConfirmEventActionAsync(
            sender,
            "Snooze allarme",
            "Sei sicuro di voler posticipare (snooze) l'allarme?");

        if (!confirmed)
            return;

        var userId = _sessionStore.UserId ?? 0;
        await ExecuteEventActionAsync(
            sender,
            eventId => _authApi.SnoozeEventAsync(eventId, userId),
            "Snooze allarme");
    }

    private async void OnSendToOperationCenterClicked(object sender, EventArgs e)
    {
        var confirmed = await ConfirmEventActionAsync(
            sender,
            "Invio a centrale operativa",
            "Sei sicuro di voler inviare l'allarme alla centrale operativa?");

        if (!confirmed)
            return;

        var userId = _sessionStore.UserId ?? 0;
        await ExecuteEventActionAsync(
            sender,
            eventId => _authApi.SendToOperationCenterAsync(eventId, userId),
            "Invio a centrale operativa");
    }

    private async Task<bool> ConfirmEventActionAsync(object sender, string title, string message)
    {
        if (sender is not Button { BindingContext: AlarmEvent })
            return false;

        return await DisplayAlert(title, message, "Sì", "No");
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
