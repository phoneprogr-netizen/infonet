using AppInfonet.Models;
using AppInfonet.Services;
using System.Collections.ObjectModel;

namespace AppInfonet.Pages
{
    [QueryProperty(nameof(Id), "id")]
    public partial class BersaglioDettaglioPage : ContentPage
    {
        private readonly IServiceProvider _services;
        private readonly SessionBersagliStore _bersagliStore;
        private readonly SessionDevicesStore _devicesStore;

        private ObservableCollection<BersaglioDeviceFlagsRow> _rows = new();

        public BersaglioDettaglioPage(SessionBersagliStore store, IServiceProvider services, SessionDevicesStore devicesStore)
        {
            InitializeComponent();
            _store = store;
            _services = services;
            _devicesStore = devicesStore;
            DevicesList.ItemsSource = _devices;
        }
        public void SetBersaglioId(int id)
        {
            _id = id;
            LoadFromStore();
        }

        private readonly SessionBersagliStore _store;

        private int _id;
        private Bersaglio? _current;
        private ObservableCollection<BersaglioDevice> _devices = new();

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                LoadFromStore();
            }
        }

        //private void LoadFromStore()
        //{
        //    _current = _store.GetById(_id);

        //    if (_current == null)
        //        return;




        //    NameEntry.Text = _current.Name;
        //    DescriptionEditor.Text = _current.Description;

        //    _devices.Clear();
        //    foreach (var d in _current.Devices)
        //        _devices.Add(d);
        //}


        private void LoadFromStore()
        {
            _current = _store.GetById(_id);
            if (_current == null) return;

            NameEntry.Text = _current.Name;
            DescriptionEditor.Text = _current.Description;

            // 1) mappa device associati al bersaglio per codperif
            var flagsByCod = (_current.Devices ?? new List<BersaglioDevice>())
                .Where(x => !string.IsNullOrWhiteSpace(x.CodPerif))
                .ToDictionary(x => x.CodPerif, x => x);

            // 2) prendo TUTTI i device disponibili (da lista dispositivi)
            var all = _devicesStore.GetAll();

            _devices.Clear();

            foreach (var dev in all)
            {
                var cod = dev.COD_PERIF;   // <-- adegua ai nomi reali del tuo model lista device
                var plate = dev.Plate;    // <-- adegua ai nomi reali del tuo model lista device

                if (string.IsNullOrWhiteSpace(cod))
                    continue;

                if (flagsByCod.TryGetValue(cod, out var f))
                {
                    // già associato: usa flag veri
                    _devices.Add(new BersaglioDevice
                    {
                        CodPerif = cod,
                        Plate = plate ?? f.Plate ?? "",
                        AlarmOnEnter = f.AlarmOnEnter,
                        AlarmOnExit = f.AlarmOnExit,
                        AlarmOnStart = f.AlarmOnStart,
                        AlarmOnStop = f.AlarmOnStop
                    });
                }
                else
                {
                    // non associato: mostra ma con flag spenti
                    _devices.Add(new BersaglioDevice
                    {
                        CodPerif = cod,
                        Plate = plate ?? "",
                        AlarmOnEnter = false,
                        AlarmOnExit = false,
                        AlarmOnStart = false,
                        AlarmOnStop = false
                    });
                }
            }
        }


        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_current == null)
                return;

            // aggiorno l’oggetto corrente con i valori UI
            _current.Name = NameEntry.Text ?? "";
            _current.Description = DescriptionEditor.Text ?? "";
            
            //tutti
            //_current.Devices = _devices.ToList();

            //oippure queli attivi
            _current.Devices = _devices
            .Where(d => d.AlarmOnEnter || d.AlarmOnExit || d.AlarmOnStart || d.AlarmOnStop)
            .ToList();

            _store.Upsert(_current);


            // salvo nello store (memoria) così tornando indietro la lista è coerente
            _store.Upsert(_current);

            // TODO: quando avrai endpoint update, qui chiami _api.UpdateBersaglio(...)
            await DisplayAlert("OK", "Bersaglio aggiornato (in memoria).", "Chiudi");
        }

        private async void OnMapClicked(object sender, EventArgs e)
        {
            var page = _services.GetRequiredService<BersaglioMapEditPage>();
            page.SetBersaglioId(_id);
            await Navigation.PushAsync(page);
        }
    }
}