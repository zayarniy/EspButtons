using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessMelody.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (_, __) => UpdateStatus();
            Closed += (_, __) => { /* Shutdown в App.xaml.cs */ };
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            ServerStatusText.Text = AppServices.IsServerRunning
                ? "Сервер: 🟢 запущен"
                : "Сервер: ⚪ остановлен";
        }

        /// <summary>Позволяет вкладкам попросить обновить статус-бар.</summary>
        public void RefreshServerStatus() => UpdateStatus();
    }
}