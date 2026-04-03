using AppInfonet.Models;
using AppInfonet.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AppInfonet.Pages
{
    public partial class DispositiviPage : ContentPage
    {
        private readonly AuthApi _authApi;
        private readonly SessionDevicesStore _devicesStore;

        public ObservableCollection<DashboardDeviceItem> Devices { get; } =
            new ObservableCollection<DashboardDeviceItem>();

        public DispositiviPage(AuthApi authApi,SessionDevicesStore devicesStore)
        {
            InitializeComponent();
            _authApi = authApi;
            _devicesStore = devicesStore;

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDevicesAsync();
        }

        private async Task LoadDevicesAsync()
        {
            try
            {
                Devices.Clear();

                var result = await _authApi.GetDashboardDevicesAsync();

                // ✅ QUI: salvo in memoria per riuso nella pagina bersaglio
                var list = result?.ItemList ?? new List<DashboardDeviceItem>(); // <-- usa il tipo reale dei tuoi item
                // ✅ QUI: salvo in memoria per riuso nella pagina bersaglio
                _devicesStore.Set(list);

                if (result?.ItemList != null)
                {
                    foreach (var item in result.ItemList)
                        Devices.Add(item);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", "Impossibile caricare i dispositivi.", "OK");
            }
        }

        // Click sul bottone Mappa in alto a destra
        private async void OnMapButtonClicked(object sender, EventArgs e)
        {
            if (Devices == null || Devices.Count == 0)
            {
                await DisplayAlert("Mappa dispositivi", "Nessun dispositivo da mostrare sulla mappa.", "OK");
                return;
            }

            // passo la lista corrente di dispositivi alla pagina mappa
            await Navigation.PushAsync(new DispositiviMapPage(Devices.ToList()));
        }
      


        private async void OnNewDeviceButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NuovoDispositivoPage());
        }

        private async void OnDeviceTapped(object sender, TappedEventArgs e)
        {
            // sender è il Frame, il suo BindingContext è il DashboardDeviceItem
            if (sender is Frame frame && frame.BindingContext is DashboardDeviceItem device)
            {
                await Navigation.PushAsync(new DispositivoDetPage(device));
            }
        }


    }
}