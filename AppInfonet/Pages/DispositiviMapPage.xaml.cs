using AppInfonet.Models;
using Microsoft.Maui.Platform;
using System.Globalization;
using System.Text;

namespace AppInfonet.Pages
{
    public partial class DispositiviMapPage : ContentPage
    {
        private readonly List<DashboardDeviceItem> _devices;

        public DispositiviMapPage(IEnumerable<DashboardDeviceItem> devices)
        {
            InitializeComponent();

            // prendo solo quelli con coordinate valide
            _devices = devices
                .Where(d => d.Latitude != 0 || d.Longitude != 0)
                .ToList();

            MapWebView.Navigating += MapWebView_Navigating;

            LoadMap();
        }

        //        private void LoadMap()
        //        {
        //            if (_devices.Count == 0)
        //                return;

        //            var markersJs = new StringBuilder();
        //            var boundsJs = new StringBuilder();

        //            foreach (var d in _devices)
        //            {
        //                string lat = d.Latitude.ToString(CultureInfo.InvariantCulture);
        //                string lon = d.Longitude.ToString(CultureInfo.InvariantCulture);

        //                // testo popup: nome mezzo/targa o codice
        //                var title = d.DisplayName ?? d.COD_PERIF;
        //                title = title.Replace("'", "\\'");

        //                var cod = d.COD_PERIF ?? "";
        //                var codEscaped = Uri.EscapeDataString(cod);

        //                markersJs.AppendLine($@"
        //            (function() {{
        //                var marker = L.marker([{lat}, {lon}]).addTo(map)
        //                    .bindPopup('<b>{title}</b>')
        //                    .openPopup();   // 👈 APRE SUBITO LA NUVOLA

        //                marker.on('click', function() {{
        //                    window.location.href = 'device://{codEscaped}';
        //                }});

        //                bounds.extend([{lat}, {lon}]);
        //            }})();");

        //                // bounds gestiti dentro lo IIFE (vedi sopra)
        //            }

        //            string html = $@"
        //<!DOCTYPE html>
        //<html>
        //<head>
        //    <meta charset='utf-8' />
        //    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        //    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.3/dist/leaflet.css' />
        //    <script src='https://unpkg.com/leaflet@1.9.3/dist/leaflet.js'></script>
        //    <style>
        //        html, body, #map {{
        //            height: 100%;
        //            margin: 0;
        //            padding: 0;
        //        }}
        //    </style>
        //</head>
        //<body>
        //    <div id='map'></div>
        //    <script>
        //        var map = L.map('map');

        //        var streets = L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
        //            maxZoom: 19,
        //            attribution: '© OpenStreetMap'
        //        }}).addTo(map);

        //        var bounds = L.latLngBounds([]);

        //        {markersJs}

        //        map.fitBounds(bounds, {{ padding: [20, 20] }});
        //    </script>
        //</body>
        //</html>";

        //            MapWebView.Source = new HtmlWebViewSource
        //            {
        //                Html = html
        //            };
        //        }

        private void LoadMap()
        {
            if (_devices.Count == 0)
                return;

            var markersJs = new StringBuilder();

            bool first = true; // se vuoi aprire il popup solo sul primo

            foreach (var d in _devices)
            {
                string lat = d.Latitude.ToString(CultureInfo.InvariantCulture);
                string lon = d.Longitude.ToString(CultureInfo.InvariantCulture);

                // Titolo visualizzato
                var title = d.DisplayName ?? d.COD_PERIF ?? "";
                title = title.Replace("'", "\\'");

                // Velocità in km/h (arrotondata)
                string speed = d.SpeedKmph.ToString("0", CultureInfo.InvariantCulture);

                // Codice periferica per la callback verso C#
                var cod = d.COD_PERIF ?? "";
                var codEscaped = Uri.EscapeDataString(cod);

                // Se quadro ON → verde, altrimenti rosso
                string iconVar = d.Quadro ? "greenIcon" : "redIcon";

                // Se vuoi aprire il popup solo sul primo dispositivo:
                var openPopup = first ? ".openPopup()" : "";
                first = false;

                markersJs.AppendLine($@"
        (function() {{
            var marker = L.marker([{lat}, {lon}], {{ icon: {iconVar} }})
                .addTo(map)
                .bindPopup('<b>{title}</b><br/>Velocità: {speed} km/h'){openPopup};

            marker.on('click', function() {{
                window.location.href = 'device://{codEscaped}';
            }});

            bounds.extend([{lat}, {lon}]);
        }})();");
            }

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
    </style>
</head>
<body>
    <div id='map'></div>
    <script>
        var map = L.map('map');

        var streets = L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        }}).addTo(map);

        // Icone colorate (verde/rosso)
        var greenIcon = new L.Icon({{
            iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
            shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.3/images/marker-shadow.png',
            iconSize: [25, 41],
            iconAnchor: [12, 41],
            popupAnchor: [1, -34],
            shadowSize: [41, 41]
        }});

        var redIcon = new L.Icon({{
            iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
            shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.3/images/marker-shadow.png',
            iconSize: [25, 41],
            iconAnchor: [12, 41],
            popupAnchor: [1, -34],
            shadowSize: [41, 41]
        }});

        var bounds = L.latLngBounds([]);

        {markersJs}

        map.fitBounds(bounds, {{ padding: [20, 20] }});
    </script>
</body>
</html>";

            MapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        }

        private async void MapWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            // intercetto le "navigate" speciali tipo device://COD_PERIF
            if (e.Url.StartsWith("device://", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;

                var codEncoded = e.Url.Substring("device://".Length);
                var cod = Uri.UnescapeDataString(codEncoded);

                var device = _devices.FirstOrDefault(d => d.COD_PERIF == cod);
                if (device != null)
                {
                    await Navigation.PushAsync(new DispositivoDetPage(device));
                }
            }
        }
    }
}