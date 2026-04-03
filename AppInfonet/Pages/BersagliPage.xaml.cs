using AppInfonet.Models;
using AppInfonet.Services;
using System.Collections.ObjectModel;

namespace AppInfonet.Pages
{
    public partial class BersagliPage : ContentPage
    {
        private readonly AuthApi _api;
        private readonly SessionBersagliStore _store;
        private readonly IServiceProvider _services;

        private ObservableCollection<Bersaglio> _items = new();

        public static readonly BindableProperty IsLoadingProperty =
      BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(BersagliPage), false);

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }


        public BersagliPage(AuthApi api, SessionBersagliStore store, IServiceProvider services)
        {
            InitializeComponent();
            _api = api;
            _store = store;
            _services = services;


            BersagliList.ItemsSource = _items;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // se store vuoto → carico, altrimenti mostro quello in memoria
            if (_store.GetAll().Count == 0)
                await LoadAsync();
            else
                RefreshFromStore();
        }

        private async void OnReloadClicked(object sender, EventArgs e)
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var list = await _api.GetBersagliAsync();
                _store.Set(list);
                RefreshFromStore();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void RefreshFromStore()
        {
            _items.Clear();
            foreach (var x in _store.GetAll())
                _items.Add(x);
        }

        // ✅ TAP (affidabile)
        private async void OnItemTapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (e.Parameter is not int id)
                    return;

                var page = _services.GetRequiredService<BersaglioDettaglioPage>();
                page.SetBersaglioId(id);

                await Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore navigazione", ex.Message, "OK");
            }
        }


        //private async void OnMapClicked(object sender, EventArgs e)
        //{
        //    // TODO: qui poi aprirai la pagina con la mappa di tutti i bersagli
        //    await DisplayAlert("Mappa", "Aprirò la mappa con tutti i bersagli.", "OK");
        //}

        private async void OnMapClicked(object sender, EventArgs e)
        {
            var page = _services.GetRequiredService<BersagliMapPage>();
            await Navigation.PushAsync(page);
        }

        private async void OnCreateTargetClicked(object sender, EventArgs e)
        {
            // TODO: aprirà la pagina di creazione bersaglio
            await DisplayAlert("Crea", "Aprirò la pagina per creare un nuovo bersaglio.", "OK");

            // Quando avrai la pagina:
            // await Shell.Current.GoToAsync(nameof(CreaBersaglioPage));
        }
    }
}