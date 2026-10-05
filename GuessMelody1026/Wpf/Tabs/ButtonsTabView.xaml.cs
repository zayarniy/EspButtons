using GuessMelody.Core.Game;
using GuessMelody.Core.Models;
using GuessMelody.Core.Storage;
using GuessMelody.Tests;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using static GuessMelody.Core.Models.ButtonInfo;

namespace GuessMelody.Wpf.Tabs
{
    public partial class ButtonsTabView : UserControl
    {
        private DispatcherTimer _uiTimer;
        private DispatcherTimer _lampTimer;

        private readonly ObservableCollection<ButtonSnapshot> _rows =
            new ObservableCollection<ButtonSnapshot>();

        public ButtonsTabView()
        {
            InitializeComponent();
            ButtonsGrid.ItemsSource = _rows;

            _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _uiTimer.Tick += (_, __) => RefreshGrid();
            _uiTimer.Start();

            _lampTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            _lampTimer.Tick += (_, __) => { _lampTimer.Stop(); 
                ButtonTestWindow buttonTestWindow = new ButtonTestWindow();
                buttonTestWindow.LampOff(); 
            };

            Loaded += (_, __) =>
            {
                // Если сервер уже запущен — подключимся к событиям
                AttachService();
                RefreshGrid();
                UpdateServerUI();
            };

            Unloaded += (_, __) =>
            {
                _uiTimer.Stop();
                _lampTimer.Stop();
                DetachService();
            };
        }

        private void RefreshGrid()
        {
            throw new NotImplementedException();
        }

        // =============================================================
        // Старт/стоп
        // =============================================================
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (AppServices.IsServerRunning) { Log("Уже запущено."); return; }

            if (!int.TryParse(PortBox.Text, out var port) || port <= 0 || port > 65535)
            { Log("Некорректный порт."); return; }

            try
            {
                AppServices.StartServer(port);
                AttachService();
                Log($"Сервер запущен на UDP:{port}");
                UpdateServerUI();
                (Window.GetWindow(this) as MainWindow)?.RefreshServerStatus();
            }
            catch (Exception ex)
            {
                Log("Ошибка старта: " + ex.Message);
                AppServices.StopServer();
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            DetachService();
            AppServices.StopServer();
            Log("Сервер остановлен.");
            UpdateServerUI();
            (Window.GetWindow(this) as MainWindow)?.RefreshServerStatus();
        }

        private void UpdateServerUI()
        {
            PortBox.IsEnabled = !AppServices.IsServerRunning;
        }

        // =============================================================
        // Подписка на события ButtonService
        // =============================================================
        private void AttachService()
        {
            var svc = AppServices.ButtonService;
            if (svc == null) return;

            svc.PressReceived += OnPressReceived;
            svc.ButtonConnected += OnButtonConnected;
            svc.ButtonReconnected += OnButtonReconnected;
            svc.ButtonDisconnected += OnButtonDisconnected;
            svc.RawLog += OnRawLog;
        }

        private void DetachService()
        {
            var svc = AppServices.ButtonService;
            if (svc == null) return;

            svc.PressReceived -= OnPressReceived;
            svc.ButtonConnected -= OnButtonConnected;
            svc.ButtonReconnected -= OnButtonReconnected;
            svc.ButtonDisconnected -= OnButtonDisconnected;
            svc.RawLog -= OnRawLog;
        }

        private void OnPressReceived(object s, PressEvent e) =>
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var snap = AppServices.ButtonService.GetByMac(e.Mac);
                var label = snap?.Label ?? e.Mac;
                Log($"🔴 PRESS: {label}  seq={e.Seq}  from={e.RemoteIp}");
                ButtonTestWindow buttonTestWindow = new ButtonTestWindow();
                buttonTestWindow.LampOn(label);
                RefreshGrid();
            }));

        private void OnButtonConnected(object s, ButtonSnapshot b) =>
            Dispatcher.BeginInvoke(new Action(() =>
                Log($"🟢 Подключилась: {b.Label} ({b.Mac})")));

        private void OnButtonReconnected(object s, ButtonSnapshot b) =>
            Dispatcher.BeginInvoke(new Action(() =>
                Log($"🔗 Вернулась:    {b.Label} ({b.Mac})")));

        private void OnButtonDisconnected(object s, ButtonSnapshot b) =>
            Dispatcher.BeginInvoke(new Action(() =>
                Log($"🔌 Потеряна:     {b.Label} ({b.Mac})")));

        private void OnRawLog(object s, string m) => Dispatcher.BeginInvoke(new Action(() => Log($"[RAW] {m}")));

        // Add this method to the ButtonsTabView class to resolve CS0103 for 'Log'
        private void Log(string message)
        {
            // Example: Output to Debug window or implement as needed
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
