using AppInfonet.Models;
using AppInfonet.Services;
using System.Text;
using System.Text.Json;

namespace AppInfonet.Pages
{
    public partial class BersaglioMapEditPage : ContentPage
    {
        private readonly SessionBersagliStore _store;

        private int _id;
        private Bersaglio? _current;

        public BersaglioMapEditPage(SessionBersagliStore store)
        {
            InitializeComponent();
            _store = store;
        }

        public void SetBersaglioId(int id)
        {
            _id = id;
            LoadFromStoreAndRender();
        }

        private void LoadFromStoreAndRender()
        {
            _current = _store.GetById(_id);
            if (_current == null)
                return;

            TargetNameLabel.Text = _current.Name;

            // array JS: [[lat, lon], [lat, lon], ...]
            var jsPoints = string.Join(", ",
                _current.PolylineArray.Select(p => $"[{p.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {p.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}]"));

            var html = BuildHtml(jsPoints);
            MapWebView.Source = new HtmlWebViewSource { Html = html };
        }

        private string BuildHtml(string jsPoints)
        {
            // Leaflet + Leaflet.draw da CDN (serve internet)
            return $@"
<!doctype html>
<html>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1.0' />

  <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
  <link rel='stylesheet' href='https://unpkg.com/leaflet-draw@1.0.4/dist/leaflet.draw.css' />

  <style>
    html, body, #map {{ height: 100%; margin: 0; padding: 0; }}
    .leaflet-container {{ background: #fff; }}
  </style>
</head>
<body>
  <div id='map'></div>

  <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
  <script src='https://unpkg.com/leaflet-draw@1.0.4/dist/leaflet.draw.js'></script>

  <script>
    var map = L.map('map');

    // basemap
    L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
      maxZoom: 19
    }}).addTo(map);

    var drawnItems = new L.FeatureGroup();
    map.addLayer(drawnItems);

    // poligono iniziale
    var initial = [{jsPoints}];
    var polygon = null;

    if (initial.length >= 3) {{
      polygon = L.polygon(initial, {{}}).addTo(drawnItems);
      map.fitBounds(polygon.getBounds(), {{ padding: [20,20] }});
    }} else {{
      // fallback centro mondo
      map.setView([45.4641, 9.19], 13);
    }}

    // toolbar draw/edit
    var drawControl = new L.Control.Draw({{
      edit: {{
        featureGroup: drawnItems,
        remove: true
      }},
      draw: {{
        polygon: true,
        polyline: false,
        rectangle: false,
        circle: false,
        circlemarker: false,
        marker: false
      }}
    }});
    map.addControl(drawControl);

    // se l'utente disegna un nuovo poligono, teniamo solo l'ultimo
    map.on(L.Draw.Event.CREATED, function (e) {{
      if (polygon) {{
        drawnItems.removeLayer(polygon);
      }}
      polygon = e.layer;
      drawnItems.addLayer(polygon);
      map.fitBounds(polygon.getBounds(), {{ padding: [20,20] }});
    }});

    // funzione chiamata da MAUI per ottenere i punti
    window.getPolygon = function() {{
      if (!polygon) return '[]';
      // Leaflet polygon latlngs: [ [ {{lat,lng}}, ... ] ]
      var latlngs = polygon.getLatLngs();
      if (!latlngs || latlngs.length === 0) return '[]';
      var ring = latlngs[0].map(p => {{ return {{ Latitude: p.lat, Longitude: p.lng }}; }});
      // opzionale: chiudere il poligono duplicando il primo punto
      if (ring.length > 2) {{
        var first = ring[0];
        var last = ring[ring.length-1];
        if (first.Latitude !== last.Latitude || first.Longitude !== last.Longitude) {{
          ring.push(first);
        }}
      }}
      return JSON.stringify(ring);
    }};
  </script>
</body>
</html>";
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_current == null) return;

            try
            {
                // ritorna una stringa JSON
                //var json = await MapWebView.EvaluateJavaScriptAsync("window.getPolygon()");
                //if (string.IsNullOrWhiteSpace(json))
                //    throw new Exception("Poligono vuoto.");

                //// a volte EvaluateJavaScriptAsync ritorna stringhe con virgolette extra
                //json = json.Trim();
                //if (json.StartsWith("\"") && json.EndsWith("\""))
                //    json = JsonSerializer.Deserialize<string>(json) ?? "[]";

                var raw = await MapWebView.EvaluateJavaScriptAsync("window.getPolygon()");
                if (string.IsNullOrWhiteSpace(raw))
                    throw new Exception("Risposta vuota dalla mappa.");


                await DisplayAlert("DEBUG RAW", raw.Length > 400 ? raw.Substring(0, 400) : raw, "OK");

                var json = NormalizeJsResultToJson(raw);

                var points = JsonSerializer.Deserialize<List<BersaglioPoint>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<BersaglioPoint>();

                if (points.Count < 3)
                    throw new Exception("Il poligono deve avere almeno 3 punti.");

                _current.PolylineArray = points;

                // opzionale: rigenero anche polyline_javascript tipo "[lat, lon], ..."
                _current.PolylineJavascript = string.Join(", ",
                    points.Select(p =>
                        $"[{p.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {p.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}]"));

                _store.Upsert(_current);

                await DisplayAlert("OK", "Poligono aggiornato.", "Chiudi");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", ex.Message, "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private static string NormalizeJsResultToJson(string raw)
        {
            raw = raw.Trim();

            // Caso A: il WebView restituisce JSON puro (inizia con [ oppure {)
            if (raw.StartsWith("[") || raw.StartsWith("{"))
                return raw;

            // Caso B: il WebView restituisce una stringa JSON tra virgolette: "...."
            // In questo caso JsonSerializer.Deserialize<string> toglie le virgolette e unescapa i backslash.
            if (raw.StartsWith("\"") && raw.EndsWith("\""))
            {
                var unescaped = JsonSerializer.Deserialize<string>(raw);
                if (string.IsNullOrWhiteSpace(unescaped))
                    throw new Exception("Impossibile leggere il JSON dalla mappa.");
                return unescaped;
            }

            // Caso C: fallback: prova comunque a interpretarlo come stringa JSON
            // (alcuni WebView possono restituire qualcosa tipo '\"[ ... ]\"')
            var fallback = JsonSerializer.Deserialize<string>($"\"{raw.Replace("\"", "\\\"")}\"");
            return fallback ?? raw;
        }

        // Non indispensabile qui; lasciato se vuoi intercettare url custom in futuro
        private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
        {
            // placeholder
        }
    }
}