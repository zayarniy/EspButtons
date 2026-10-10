using System;
using System.Diagnostics;
using System.Windows.Input;
using GuessMelody.AnswerServer;
using GuessMelody.Core;
using GuessMelody.Core.Enums;
using GuessMelody.Services;

namespace GuessMelody.ViewModels
{
    public class AnswerServerTabViewModel : ViewModelBase, IDisposable
    {
        private readonly AnswerHttpServer _server;
        private readonly LogService _log;
        private readonly DialogService _dlg;

        public AnswerServerTabViewModel(LogService log, DialogService dlg)
        {
            _log = log;
            _dlg = dlg;
            _server = new AnswerHttpServer();
            _server.Log += (s, line) => _log.Add(LogKind.Http, line);

            StartCommand = new RelayCommand(_ => Start(), _ => !IsRunning);
            StopCommand = new RelayCommand(_ => Stop(), _ => IsRunning);
            OpenBrowserCommand = new RelayCommand(_ => OpenBrowser());
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenBrowserCommand { get; }

        private int _port = 8080;
        public int Port { get => _port; set => Set(ref _port, value); }

        private string _title = "Угадай мелодию";
        public string Title { get => _title; set { Set(ref _title, value); _server.Title = value; } }

        private string _placeholder = "";
        public string Placeholder
        {
            get => _placeholder;
            set { Set(ref _placeholder, value); _server.PlaceholderText = value; }
        }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; private set => Set(ref _isRunning, value); }

        public string Url => $"http://localhost:{Port}/";

        private void Start()
        {
            _server.Title = Title;
            _server.PlaceholderText = Placeholder;
            _server.Start(Port);
            IsRunning = _server.IsRunning;
            OnPropertyChanged(nameof(Url));
        }

        private void Stop()
        {
            _server.Stop();
            IsRunning = false;
        }

        private void OpenBrowser()
        {
            try { Process.Start(Url); }
            catch (Exception ex) { _dlg.Error(ex.Message); }
        }

        public void Dispose() => _server?.Dispose();
    }
}