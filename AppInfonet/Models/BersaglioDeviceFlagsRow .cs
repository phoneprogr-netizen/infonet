using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppInfonet.Models
{
    public class BersaglioDeviceFlagsRow : INotifyPropertyChanged
    {
        private bool _alarmOnEnter;
        private bool _alarmOnExit;
        private bool _alarmOnStart;
        private bool _alarmOnStop;

        public string CodPerif { get; set; } = "";
        public string Plate { get; set; } = "";

        public bool AlarmOnEnter { get => _alarmOnEnter; set { _alarmOnEnter = value; OnPropertyChanged(); } }
        public bool AlarmOnExit { get => _alarmOnExit; set { _alarmOnExit = value; OnPropertyChanged(); } }
        public bool AlarmOnStart { get => _alarmOnStart; set { _alarmOnStart = value; OnPropertyChanged(); } }
        public bool AlarmOnStop { get => _alarmOnStop; set { _alarmOnStop = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}