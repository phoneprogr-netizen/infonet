using AppInfonet.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace AppInfonet.Pages
{
    public partial class BersagliMapPage : ContentPage
    {
        private readonly SessionBersagliStore _store;
        private readonly IServiceProvider _services;

        public BersagliMapPage(SessionBersagliStore store, IServiceProvider services)
        {
            InitializeComponent();
            _store = store;
            _services = services;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RenderMap();
        }

        private void RenderMap()
        {
            var bersagli = _store.GetAll();

            // Creo array JS di oggetti: { id, name, points:[[lat,lon],...] }
            string EscapeJs(string s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "\\'");

            var itemsJs = string.Join(",\n",
                bersagli.Select(b =>
                {
                    var pts = (b.PolylineArray ?? new())
                        .Select(p => $"[{p.Latitude.ToString(CultureInfo.InvariantCulture)}, {p.Longitude.ToString(CultureInfo.InvariantCulture)}]");
                    var ptsJs = string.Join(",", pts);

                    return $"{{ id: {b.Id}, name: '{EscapeJs(b.Name)}', points: [{ptsJs}] }}";
                }));

            var html = BuildHtml(itemsJs);
            MapWebView.Source = new HtmlWebViewSource { Html = html };
        }

        private string BuildHtml(string itemsJs)
        {
            // IMPORTANT: in stringa interpolata C# ogni { } JS va raddoppiata {{ }}
            return $@"
<!doctype html>
<html>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1.0' />

  <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
  <style>
    html, body, #map {{ height: 100%; margin: 0; padding: 0; }}
  </style>
</head>
<body>
  <div id='map'></div>

  <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
  <script>
    var map = L.map('map');

    L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
      maxZoom: 19
    }}).addTo(map);

    var items = [{itemsJs}];

    var bounds = null;

    items.forEach(function(it) {{
      if (!it.points || it.points.length < 3) return;

      var poly = L.polygon(it.points).addTo(map);

      // tooltip col nome
      poly.bindTooltip(it.name, {{ sticky: true }});

      // click sul poligono -> manda evento a MAUI via url custom
      poly.on('click', function() {{
        window.location.href = 'app://bersaglio?id=' + it.id;
      }});

      // estendo bounds
      var b = poly.getBounds();
      bounds = bounds ? bounds.extend(b) : b;
    }});

    if (bounds) {{
      map.fitBounds(bounds, {{ padding: [20, 20] }});
    }} else {{
      map.setView([45.4641, 9.19], 6);
    }}
  </script>
</body>
</html>";
        }

        private async void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
        {
            // intercetto: app://bersaglio?id=3440
            if (e.Url != null && e.Url.StartsWith("app://bersaglio", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;

                try
                {
                    var uri = new Uri(e.Url);
                    var query = uri.Query; // ?id=xxxx
                    var idStr = System.Web.HttpUtility.ParseQueryString(query).Get("id");

                    if (int.TryParse(idStr, out var id))
                    {
                        var page = _services.GetRequiredService<BersaglioDettaglioPage>();
                        page.SetBersaglioId(id);
                        await Navigation.PushAsync(page);
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Errore", ex.Message, "OK");
                }
            }
        }
    }
}