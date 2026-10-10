using GuessMelody.Core;
using GuessMelody.Core.Buttons;
using GuessMelody.Core.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using GuessMelody.Buttons;


namespace GuessMelody.Buttons
{
    public class ButtonService : IDisposable
    {
        private readonly EspButtonHub _hub;
        private readonly LogService _log;
        private readonly Dictionary<string, string> _macToName =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ButtonsConfig Config { get; private set; } = new ButtonsConfig();
        public EspButtonHub Hub => _hub;

        public event EventHandler<PressEventArgs> Press;
        public event EventHandler<ButtonEventArgs> Connected;
        public event EventHandler<ButtonEventArgs> Reconnected;
        public event EventHandler<ButtonEventArgs> Disconnected;
        public event EventHandler<AckEventArgs> Ack;

        public ButtonService(LogService log,
                             int listenPort = 41234,
                             int ackPort = 41235,
                             int cmdPort = 41234)
        {
            _log = log;
            _hub = new EspButtonHub(listenPort, ackPort, cmdPort);

            _hub.RawLog += (s, line) => _log.Add(LogKind.Raw, line);
            _hub.ButtonConnected += (s, e) => Connected?.Invoke(this, e);
            _hub.ButtonReconnected += (s, e) => Reconnected?.Invoke(this, e);
            _hub.ButtonDisconnected += (s, e) => Disconnected?.Invoke(this, e);
            _hub.PressReceived += (s, e) => Press?.Invoke(this, e);
            _hub.CommandAck += (s, e) => Ack?.Invoke(this, e);
        }

        public void Start() => _hub.Start();
        public void Stop() => _hub.Stop();

        public void ApplyBindings(ButtonsConfig cfg)
        {
            Config = cfg ?? new ButtonsConfig();
            _macToName.Clear();
            foreach (var b in Config.Bindings)
                _macToName[b.Mac] = b.TeamName;
            _log.Add(LogKind.System, $"Привязки кнопок применены ({Config.Bindings.Count})");
        }

        public string GetTeamName(string mac) =>
            _macToName.TryGetValue(mac, out var name) ? name : mac;

        public void SendSetServer(IPAddress ip, IEnumerable<string> macs = null) =>
            _hub.SendSetServer(ip, macs);

        public void SendSetWifi(string ssid, string password, IEnumerable<string> macs = null) =>
            _hub.SendSetWifi(ssid, password, macs);

        public void SendReboot(IEnumerable<string> macs = null) =>
            _hub.SendReboot(macs);

        public void Dispose() => _hub?.Dispose();
    }
}