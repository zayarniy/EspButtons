using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GuessMelody.Core;
using GuessMelody.Core.Enums;
using GuessMelody.Core.Models;
using GuessMelody.Core.Serialization;
using GuessMelody.Services;

namespace GuessMelody.ViewModels
{
    public class GameTabViewModel : ViewModelBase
    {
        private readonly LogService _log;
        private readonly GameEngine _engine;
        private readonly DialogService _dlg;
        private readonly RecentFilesService _recent;
        private readonly AudioCoordinator _audio;

        public ObservableCollection<string> RecentFiles =>
            new ObservableCollection<string>(_recent.Files);

        public GameTabViewModel(
            LogService log,
            GameEngine engine,
            DialogService dlg,
            RecentFilesService recent,
            AudioCoordinator audio)
        {
            _log = log;
            _engine = engine;
            _dlg = dlg;
            _recent = recent;
            _audio = audio;

            StartCommand = new RelayCommand(_ => StartServer(), _ => !IsRunning);
            StopCommand = new RelayCommand(_ => StopServer(), _ => IsRunning);
            LoadGameCommand = new RelayCommand(_ => LoadGame());
            SaveGameCommand = new RelayCommand(_ => SaveGame(false));
            SaveGameAsCommand = new RelayCommand(_ => SaveGame(true));

            OpenPlayerCommand = new RelayCommand(_ => OpenPlayer?.Invoke(this, EventArgs.Empty));
            OpenHostCommand = new RelayCommand(_ => OpenHost?.Invoke(this, EventArgs.Empty));

            TestPressCommand = new RelayCommand(p => TestPress(p as string));
            TestRoundCommand = new RelayCommand(p => TestRound(p as string));
            TestScoreYesCommand = new RelayCommand(_ => TestScore(true));
            TestScoreNoCommand = new RelayCommand(_ => TestScore(false));
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand LoadGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand SaveGameAsCommand { get; }
        public ICommand OpenPlayerCommand { get; }
        public ICommand OpenHostCommand { get; }
        public ICommand TestPressCommand { get; }
        public ICommand TestRoundCommand { get; }
        public ICommand TestScoreYesCommand { get; }
        public ICommand TestScoreNoCommand { get; }

        public event EventHandler OpenPlayer;
        public event EventHandler OpenHost;

        // Ссылки на VM, чтобы дергать их из GameTab
        public ButtonsTabViewModel Buttons { get; set; }
        public SettingsTabViewModel Settings { get; set; }
        public FoldersTabViewModel Folders { get; set; }

        private int _udpPort = 41234;
        public int UdpPort { get => _udpPort; set => Set(ref _udpPort, value); }

        private string _serverIp = "192.168.137.1";
        public string ServerIp { get => _serverIp; set => Set(ref _serverIp, value); }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; private set => Set(ref _isRunning, value); }

        private string _statusText = "Сервер не запущен";
        public string StatusText { get => _statusText; private set => Set(ref _statusText, value); }

        private void StartServer()
        {
            if (Buttons == null) { _dlg.Warn("Вкладка кнопок не подключена."); return; }
            Buttons.UdpPort = UdpPort;
            Buttons.StartCommand.Execute(null);
            IsRunning = Buttons.IsRunning;
            StatusText = Buttons.StatusText;
        }

        private void StopServer()
        {
            if (Buttons == null) return;
            Buttons.StopCommand.Execute(null);
            IsRunning = Buttons.IsRunning;
            StatusText = Buttons.StatusText;
        }

        private void LoadGame()
        {
            var path = _dlg.OpenFile("GuessMelody game (*.gmgame)|*.gmgame|JSON (*.json)|*.json");
            if (string.IsNullOrEmpty(path)) return;

            var s = JsonStore.Load<GameSettings>(path);
            if (s == null) { _dlg.Error("Не удалось прочитать файл."); return; }

            _engine.ApplySettings(s);
            Settings?.ApplyFromSettings(s);
            Folders?.ApplyFromSettings(s);
            Buttons?.RefreshFromHub();

            _audio.RootFolder = s.Folders?.RootFolder ?? "";

            _recent.Add(path);
            OnPropertyChanged(nameof(RecentFiles));
            _log.Add(LogKind.System, $"Игра загружена: {path}");
        }

        private void SaveGame(bool asNew)
        {
            var path = asNew
                ? _dlg.SaveFile("GuessMelody game (*.gmgame)|*.gmgame", "game.gmgame")
                : _dlg.SaveFile("GuessMelody game (*.gmgame)|*.gmgame", "game.gmgame");

            if (string.IsNullOrEmpty(path)) return;

            Settings?.ApplyToSettings(_engine.Settings);
            Folders?.ApplyToSettings(_engine.Settings);

            if (JsonStore.Save(path, _engine.Settings))
            {
                _recent.Add(path);
                OnPropertyChanged(nameof(RecentFiles));
                _log.Add(LogKind.System, $"Игра сохранена: {path}");
            }
            else
            {
                _dlg.Error("Не удалось сохранить игру.");
            }
        }

        private void TestPress(string teamId)
        {
            if (!int.TryParse(teamId, out var n)) return;
            var team = _engine.Teams.FirstOrDefault(t => t.Id == n);
            if (team == null) return;
            _engine.OnPlayerPress(team.Mac, DateTime.UtcNow, "test", 0);
            _log.Add(LogKind.Game, $"[TEST] Нажатие команды {teamId}");
        }

        private void TestRound(string roundNo)
        {
            if (!int.TryParse(roundNo, out var n)) return;
            _engine.StartRound(n - 1);
            _log.Add(LogKind.Game, $"[TEST] Раунд {roundNo}");
        }

        private void TestScore(bool yes)
        {
            _engine.SubmitScore(yes);
            _log.Add(LogKind.Game, $"[TEST] Очки: {(yes ? "Да" : "Нет")}");
        }
    }
}