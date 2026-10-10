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
            SaveGameCommand = new RelayCommand(_ => SaveGame());
            SaveGameAsCommand = new RelayCommand(_ => SaveGameAs());

            OpenPlayerCommand = new RelayCommand(_ => OpenPlayer?.Invoke(this, EventArgs.Empty));
            OpenHostCommand = new RelayCommand(_ => OpenHost?.Invoke(this, EventArgs.Empty));

            TestPressCommand = new RelayCommand(p =>
            {
                if (int.TryParse(p as string, out var id))
                    _engine.EmulatePress(id);
            });

            TestRoundCommand = new RelayCommand(p =>
            {
                if (int.TryParse(p as string, out var n))
                    _engine.StartRound(n - 1);   // 1..9 → индекс категории
            });

            TestScoreYesCommand = new RelayCommand(_ => _engine.SubmitScore(true));
            TestScoreNoCommand = new RelayCommand(_ => _engine.SubmitScore(false));

            ResetScoresCommand = new RelayCommand(_ => _engine.ResetScores());
            FillDemoCommand = new RelayCommand(_ => FillDemo());
            QuickDemoCommand = new RelayCommand(_ => QuickDemo());
            NextRoundCommand = new RelayCommand(_ => _engine.NextRound());
            PanicCommand = new RelayCommand(_ => _engine.Panic());
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
        public ICommand ResetScoresCommand { get; }
        public ICommand FillDemoCommand { get; }
        public ICommand QuickDemoCommand { get; }
        public ICommand NextRoundCommand { get; }
        public ICommand PanicCommand { get; }

        public event EventHandler OpenPlayer;
        public event EventHandler OpenHost;

        // Ссылки на соседние VM — присваиваются в MainViewModel
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

        // --- состояние движка, для биндинга в UI ---
        public string RoundText =>
            _engine.TotalRounds > 0
                ? $"Раунд {_engine.CurrentRound + 1} / {_engine.TotalRounds}"
                : $"Раунд {_engine.CurrentRound + 1}";

        public string StateText
        {
            get
            {
                switch (_engine.State)
                {
                    case RoundState.Idle: return "Ожидание";
                    case RoundState.Countdown: return $"Отсчёт {_engine.CountdownLeftSec:F0}";
                    case RoundState.Playing: return "🎵 Играет";
                    case RoundState.WaitingForAnswer:
                        return _engine.FirstPressedTeam != null
                                                        ? $"Первый: {_engine.FirstPressedTeam.Name}"
                                                        : "Ждём ответа";
                    case RoundState.Scored: return "Очко начислено";
                    case RoundState.Finished: return "Завершено";
                }
                return "";
            }
        }

        public string FirstPressedText => _engine.FirstPressedTeam?.Name ?? "—";

        public void HookEngineEvents()
        {
            _engine.StateChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(RoundText));
                OnPropertyChanged(nameof(StateText));
                OnPropertyChanged(nameof(FirstPressedText));
            };
            _engine.ScoreChanged += (s, e) => OnPropertyChanged(nameof(FirstPressedText));
        }

        // -------------------------------------------------------------
        // Сервер
        // -------------------------------------------------------------
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

        // -------------------------------------------------------------
        // Load/Save игры
        // -------------------------------------------------------------
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

        private void SaveGame() => SaveGameTo(null);
        private void SaveGameAs() => SaveGameTo("game.gmgame");

        private void SaveGameTo(string defaultName)
        {
            string path;
            if (string.IsNullOrEmpty(defaultName))
            {
                path = _dlg.SaveFile("GuessMelody game (*.gmgame)|*.gmgame",
                    string.IsNullOrEmpty(_engine.Settings.Name)
                        ? "game.gmgame"
                        : _engine.Settings.Name + ".gmgame");
            }
            else
            {
                path = _dlg.SaveFile("GuessMelody game (*.gmgame)|*.gmgame", defaultName);
            }
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

        // -------------------------------------------------------------
        // Быстрые сценарии
        // -------------------------------------------------------------

        /// <summary>Заполнить игроков 1..6 и категории-заглушки, если пусто.</summary>
        private void FillDemo()
        {
            var s = _engine.Settings;

            // Игроки 1..3, если никого нет
            if (s.Buttons?.Teams == null || s.Buttons.Teams.Count == 0)
            {
                if (s.Buttons == null) s.Buttons = new ButtonsMap();
                for (int i = 1; i <= 3; i++)
                    _engine.EnsureTeam(i, $"Игрок {i}");
            }

            // Категории-заглушки, если папок нет
            if (s.Folders?.Categories == null || s.Folders.Categories.Count == 0)
            {
                if (s.Folders == null) s.Folders = new FoldersCatalog();
                for (int i = 1; i <= 3; i++)
                {
                    var cat = new Category { Name = $"Категория {i}" };
                    for (int j = 1; j <= 3; j++)
                        cat.Tracks.Add(new Track { RelativePath = $"demo/cat{i}/track{j}.mp3" });
                    s.Folders.Categories.Add(cat);
                }
                _log.Add(LogKind.System, "Заполнены демо-категории (без реальных файлов).");
            }

            _dlg.Info("Заполнено: 3 игрока (VIRT), 3 категории по 3 трека.\n" +
                      "Файлы не существуют — при старте раунда музыка не заиграет, " +
                      "но весь цикл раунда будет работать.");

            // Обновить оба окна
            OpenPlayer?.Invoke(this, EventArgs.Empty);
            OpenHost?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Заполнить демо-данные и сразу стартовать 1-й раунд.</summary>
        private void QuickDemo()
        {
            FillDemo();
            // Стартуем 1-ю категорию
            _engine.StartRound(0);
        }
    }
}