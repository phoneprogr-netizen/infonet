using AppInfonet.Models;

namespace AppInfonet.Services
{
    public class SessionDevicesStore
    {
        private List<DashboardDeviceItem> _items = new();

        public void Set(IEnumerable<DashboardDeviceItem> items)
        {
            _items = items?.ToList() ?? new List<DashboardDeviceItem>();
        }

        public IReadOnlyList<DashboardDeviceItem> GetAll() => _items;

        public DashboardDeviceItem? GetByCodPerif(string codperif)
            => _items.FirstOrDefault(d => d.COD_PERIF == codperif);
    }
}