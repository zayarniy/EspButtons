using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EspButtonDiag.Wpf.Models
{
    public class ButtonRowViewModel : INotifyPropertyChanged
    {
        public string Mac { get; }
        public string FirstSeenLocal => _firstSeen.ToLocalTime().ToString("HH:mm:ss");
        public string LastSeenLocal => _lastSeen.ToLocalTime().ToString("HH:mm:ss");
        public string LastIp { get; private set; }
        public bool Alive { get; private set; }
        public int PressCount { get; private set; }
        public string LastSeq { get; private set; }
        public long LastUptimeMs { get; private set; }

        public string SecondsSinceLastSeen =>
            (DateTime.UtcNow - _lastSeen).TotalSeconds.ToString("F1");

        private DateTime _firstSeen;
        private DateTime _lastSeen;

        public ButtonRowViewModel(string mac)
        {
            Mac = mac;
            _firstSeen = DateTime.UtcNow;
            _lastSeen = DateTime.UtcNow;
            Alive = true;
            LastIp = "";
            LastSeq = "";
        }

        public void Update(ButtonInfo info)
        {
            _firstSeen = info.FirstSeenUtc;
            _lastSeen = info.LastSeenUtc;
            LastIp = info.LastIp?.ToString() ?? "";
            Alive = info.Alive;
            PressCount = info.PressCount;
            LastSeq = info.LastSeq ?? "";
            LastUptimeMs = info.LastUptimeMs;

            OnPropertyChanged(nameof(FirstSeenLocal));
            OnPropertyChanged(nameof(LastSeenLocal));
            OnPropertyChanged(nameof(LastIp));
            OnPropertyChanged(nameof(Alive));
            OnPropertyChanged(nameof(PressCount));
            OnPropertyChanged(nameof(LastSeq));
            OnPropertyChanged(nameof(LastUptimeMs));
            OnPropertyChanged(nameof(SecondsSinceLastSeen));
        }

        // Для таймера, обновляющего "сколько секунд назад"
        public void RefreshTime() => OnPropertyChanged(nameof(SecondsSinceLastSeen));

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}