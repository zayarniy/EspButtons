using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using EspButtonDiag.Wpf.Models;
using EspButtonDiag.Wpf.Utils;   // RelayCommand

namespace EspButtonDiag.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly EspButtonHub _hub;
        private readonly Dispatcher _disp;
        private DispatcherTimer _uiTimer;

        public ObservableCollection<ButtonRowViewModel> Buttons { get; }
            = new ObservableCollection<ButtonRowViewModel>();

        public ObservableCollection<LogEntry> Log { get; }
            = new ObservableCollection<LogEntry>();

        public int LogLimit { get; set; } = 5000;

        // -------- Свойства для UI --------
        private string _statusText = "Остановлено";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value; OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int _listenPort = 41234;
        public int ListenPort
        {
            get => _listenPort;
            set { _listenPort = value; OnPropertyChanged(); }
        }

        private int _ackPort = 41235;
        public int AckPort
        {
            get => _ackPort;
            set { _ackPort = value; OnPropertyChanged(); }
        }

        private int _heartbeatTimeoutSec = 30;
        public int HeartbeatTimeoutSec
        {
            get => _heartbeatTimeoutSec;
            set { _heartbeatTimeoutSec = value; OnPropertyChanged(); }
        }

        // -------- Команды --------
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand ClearButtonsCommand { get; }

        public MainViewModel()
        {
            _disp = Application.Current.Dispatcher;

            StartCommand = new RelayCommand(_ => StartServer(),
                _ => !IsRunning);

            StopCommand = new RelayCommand(_ => StopServer(),
                _ => IsRunning);

            ClearLogCommand = new RelayCommand(_ => Log.Clear());

            ClearButtonsCommand = new RelayCommand(_ =>
            {
                Buttons.Clear();
            }, _ => !IsRunning);

            _hub = new EspButtonHub(_listenPort, _ackPort,
                heartbeatTimeout: TimeSpan.FromSeconds(_heartbeatTimeoutSec));

            _hub.RawLog += Hub_RawLog;
            _hub.ButtonConnected += Hub_ButtonConnected;
            _hub.ButtonReconnected += Hub_ButtonReconnected;
            _hub.ButtonDisconnected += Hub_ButtonDisconnected;
            _hub.PressReceived += Hub_PressReceived;

            // Таймер для обновления "сколько секунд назад" в таблице
            _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _uiTimer.Tick += (_, __) =>
            {
                foreach (var b in Buttons) b.RefreshTime();
            };
            _uiTimer.Start();
        }

        // -------------------------------------------------------------
        private void StartServer()
        {
            try
            {
                // Пересоздаём hub с актуальными портами/таймаутом
                if (IsRunning) return;

                _hub.Start();
                IsRunning = true;
                StatusText = $"Запущено. UDP:{_listenPort} → ACK:{_ackPort}";

                AddLog(LogKind.System,
                    $"Сервер запущен на UDP:{_listenPort}, ACK порт {_ackPort}, " +
                    $"таймаут heartbeat {_heartbeatTimeoutSec} с");
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка запуска";
                AddLog(LogKind.Error, $"Start error: {ex.Message}");
                MessageBox.Show(ex.Message, "Ошибка запуска",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopServer()
        {
            try
            {
                _hub.Stop();
                IsRunning = false;
                StatusText = "Остановлено";
                AddLog(LogKind.System, "Сервер остановлен.");
            }
            catch (Exception ex)
            {
                AddLog(LogKind.Error, $"Stop error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------
        // Обработчики событий EspButtonHub (в фоновом потоке!)
        // -------------------------------------------------------------
        private void Hub_RawLog(object sender, string line) =>
            _disp.BeginInvoke(new Action(() => AddLog(LogKind.Raw, line)));

        private void Hub_ButtonConnected(object sender, ButtonEventArgs e) =>
            _disp.BeginInvoke(new Action(() =>
            {
                UpsertButton(e.Button);
                AddLog(LogKind.Connect,
                    $"НОВАЯ КНОПКА: {e.Button.Mac}  ip={e.Button.LastIp}");
                UpdateStatus();
            }));

        private void Hub_ButtonReconnected(object sender, ButtonEventArgs e) =>
            _disp.BeginInvoke(new Action(() =>
            {
                UpsertButton(e.Button);
                AddLog(LogKind.Reconnect,
                    $"ВЕРНУЛАСЬ: {e.Button.Mac}  ip={e.Button.LastIp}");
                UpdateStatus();
            }));

        private void Hub_ButtonDisconnected(object sender, ButtonEventArgs e) =>
            _disp.BeginInvoke(new Action(() =>
            {
                UpsertButton(e.Button);
                AddLog(LogKind.Disconnect,
                    $"ПОТЕРЯНА: {e.Button.Mac}  " +
                    $"(тишина {e.Button.TimeSinceLastSeen.TotalSeconds:F1} с)");
                UpdateStatus();
            }));

        private void Hub_PressReceived(object sender, PressEventArgs e) =>
            _disp.BeginInvoke(new Action(() =>
            {
                UpsertButton(e.Button);
                AddLog(LogKind.Press,
                    $"НАЖАТИЕ: {e.Button.Mac}  seq={e.Seq}  " +
                    $"uptime={e.UptimeMs} ms  from={e.RemoteIp}");
                UpdateStatus();
            }));

        // -------------------------------------------------------------
        private void UpsertButton(ButtonInfo info)
        {
            var row = Buttons.FirstOrDefault(b =>
                string.Equals(b.Mac, info.Mac, StringComparison.OrdinalIgnoreCase));

            if (row == null)
            {
                row = new ButtonRowViewModel(info.Mac);
                Buttons.Add(row);
            }
            row.Update(info);
        }

        private void UpdateStatus()
        {
            int alive = Buttons.Count(b => b.Alive);
            StatusText = $"Живых {alive}/{Buttons.Count}  •  UDP:{_listenPort}";
        }

        private void AddLog(LogKind kind, string message)
        {
            var entry = new LogEntry(DateTime.Now, kind, message);
            Log.Add(entry);

            // Ограничиваем размер
            while (Log.Count > LogLimit) Log.RemoveAt(0);
        }

        // -------------------------------------------------------------
        public void Dispose()
        {
            _uiTimer?.Stop();
            try { _hub?.Dispose(); } catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    // ----------------------------------------------------------------
    public enum LogKind
    {
        Raw,        // ← / → из EspButtonHub
        Connect,
        Reconnect,
        Disconnect,
        Press,
        System,
        Error
    }

    public class LogEntry
    {
        public DateTime Time { get; }
        public LogKind Kind { get; }
        public string Message { get; }
        public string TimeText => Time.ToString("HH:mm:ss.fff");
        public string KindText => Kind.ToString();

        public LogEntry(DateTime t, LogKind k, string m)
        {
            Time = t; Kind = k; Message = m;
        }
    }
}