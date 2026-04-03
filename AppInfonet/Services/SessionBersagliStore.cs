using AppInfonet.Models;

namespace AppInfonet.Services
{
    public class SessionBersagliStore
    {
        private List<Bersaglio> _items = new();

        public void Set(IEnumerable<Bersaglio> items)
        {
            _items = items?.ToList() ?? new List<Bersaglio>();
        }

        public IReadOnlyList<Bersaglio> GetAll() => _items;

        public Bersaglio? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

        public void Upsert(Bersaglio updated)
        {
            var idx = _items.FindIndex(x => x.Id == updated.Id);
            if (idx >= 0) _items[idx] = updated;
            else _items.Add(updated);
        }
    }
}