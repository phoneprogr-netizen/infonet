using AppInfonet.Models;
using System.Globalization;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;

namespace AppInfonet.Pages
{
    public partial class DispositivoDetPage : ContentPage
    {
        private readonly DashboardDeviceItem _device;

        public string PlateTitle =>
            !string.IsNullOrWhiteSpace(_device.Plate) ? _device.Plate! : _device.COD_PERIF;

        public string DeviceNameTitle =>
            !string.IsNullOrWhiteSpace(_device.DeviceName) ? _device.DeviceName! : "Dispositivo";

        public string FullAddressLabel =>
            !string.IsNullOrWhiteSpace(_device.FullAddress) ? _device.FullAddress! : "Indirizzo non disponibile";

        public string QuadroValue => _device.Quadro ? "ON" : "OFF";

        public string SpeedValue => $"{_device.SpeedKmph:0} km/h";

        public string BatteryValue => $"{_device.Batteria:0.0} V";

        public DispositivoDetPage(DashboardDeviceItem device)
        {
            InitializeComponent();

            _device = device;

            BindingContext = this;

            Title = PlateTitle;

            LoadMap();
        }

        private void LoadMap()
        {
            if (_device.Latitude == 0 && _device.Longitude == 0)
                return;

            string lat = _device.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = _device.Longitude.ToString(CultureInfo.InvariantCulture);

            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.3/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.9.3/dist/leaflet.js'></script>
    <style>
        html, body, #map {{
            height: 100%;
            margin: 0;
            padding: 0;
        }}
        .leaflet-control-layers-expanded {{
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div id='map'></div>
    <script>
        var streets = L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        }});

        var satellite = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{{z}}/{{y}}/{{x}}', {{
            maxZoom: 19,
            attribution: 'Tiles © Esri & contributors'
        }});

        var map = L.map('map', {{
            center: [{lat}, {lon}],
            zoom: 15,
            layers: [streets]
        }});

        L.marker([{lat}, {lon}]).addTo(map);

        var baseMaps = {{
            'Stradale': streets,
            'Satellite': satellite
        }};

        L.control.layers(baseMaps).addTo(map);
    </script>
</body>
</html>";

            DeviceMapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        }

        private async void OnOpenInMapsClicked(object sender, EventArgs e)
        {
            if (_device.Latitude == 0 && _device.Longitude == 0)
            {
                await DisplayAlert("Posizione non disponibile",
                    "Per questo dispositivo non è presente una posizione valida.",
                    "OK");
                return;
            }

            string lat = _device.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = _device.Longitude.ToString(CultureInfo.InvariantCulture);
            string label = Uri.EscapeDataString(_device.DisplayName ?? "Dispositivo");

            Uri uri;

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                uri = new Uri($"http://maps.apple.com/?ll={lat},{lon}&q={label}");
            }
            else if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                uri = new Uri($"geo:{lat},{lon}?q={lat},{lon}({label})");
            }
            else
            {
                uri = new Uri($"https://www.google.com/maps/search/?api=1&query={lat},{lon}");
            }

            await Launcher.OpenAsync(uri);
        }

        private async void OnIngressiUsciteClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new DispositivoIngressiUscitePage(_device));
        }

        private async void OnStoricoAllarmiClicked(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new DispositivoStoricoAllarmiPage(_device));
        }
    }
}
