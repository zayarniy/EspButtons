using GuessMelody.Core.Models;

namespace GuessMelody.ViewModels
{
    public class SettingsTabViewModel : ViewModelBase
    {
        // Игра
        public int PlayerCount { get => _playerCount; set => Set(ref _playerCount, value); }
        private int _playerCount = 3;
        public int RoundCount { get => _roundCount; set => Set(ref _roundCount, value); }
        private int _roundCount = 5;
        public int FragmentSec { get => _fragmentSec; set => Set(ref _fragmentSec, value); }
        private int _fragmentSec = 20;
        public int DefaultStartSec { get => _defaultStart; set => Set(ref _defaultStart, value); }
        private int _defaultStart = 0;
        public bool RandomStart { get => _randomStart; set => Set(ref _randomStart, value); }
        private bool _randomStart = false;
        public bool CountdownOnAnswer { get => _countdown; set => Set(ref _countdown, value); }
        private bool _countdown = true;
        public bool IsSetupMode { get => _setupMode; set => Set(ref _setupMode, value); }
        private bool _setupMode = false;
        public bool AllowRePress { get => _allowRePress; set => Set(ref _allowRePress, value); }
        private bool _allowRePress = false;
        public int ScorePerCorrect { get => _scorePer; set => Set(ref _scorePer, value); }
        private int _scorePer = 1;

        // Звуки
        public int Volume { get => _volume; set => Set(ref _volume, value); }
        private int _volume = 80;
        public string BuzzerPath { get => _buzzerPath; set => Set(ref _buzzerPath, value); }
        private string _buzzerPath = "";
        public string CorrectPath { get => _correctPath; set => Set(ref _correctPath, value); }
        private string _correctPath = "";
        public string WrongPath { get => _wrongPath; set => Set(ref _wrongPath, value); }
        private string _wrongPath = "";
        public string TickPath { get => _tickPath; set => Set(ref _tickPath, value); }
        private string _tickPath = "";
        public int FadeInMs { get => _fadeIn; set => Set(ref _fadeIn, value); }
        private int _fadeIn = 100;
        public int FadeOutMs { get => _fadeOut; set => Set(ref _fadeOut, value); }
        private int _fadeOut = 100;
        public bool TickLast5 { get => _tick5; set => Set(ref _tick5, value); }
        private bool _tick5 = true;

        // Тайминги
        public int CountdownBeforeSec { get => _cdBefore; set => Set(ref _cdBefore, value); }
        private int _cdBefore = 3;
        public int AnswerTimeoutSec { get => _answerTimeout; set => Set(ref _answerTimeout, value); }
        private int _answerTimeout = 30;
        public int PauseAfterPressMs { get => _pausePress; set => Set(ref _pausePress, value); }
        private int _pausePress = 0;
        public int PauseBetweenRoundsSec { get => _pauseRounds; set => Set(ref _pauseRounds, value); }
        private int _pauseRounds = 3;
        public bool AutoNextRound { get => _autoNext; set => Set(ref _autoNext, value); }
        private bool _autoNext = false;

        // Внешний вид
        public string PlayerBgColor { get => _bg; set => Set(ref _bg, value); }
        private string _bg = "#1E1E2E";
        public string PlayerAccentColor { get => _accent; set => Set(ref _accent, value); }
        private string _accent = "#FFD166";
        public string PlayerTextColor { get => _text; set => Set(ref _text, value); }
        private string _text = "#EEEEEE";
        public int CategoryFontSize { get => _catFont; set => Set(ref _catFont, value); }
        private int _catFont = 32;
        public int PlayerNameFontSize { get => _nameFont; set => Set(ref _nameFont, value); }
        private int _nameFont = 28;
        public int PlayerScoreFontSize { get => _scoreFont; set => Set(ref _scoreFont, value); }
        private int _scoreFont = 40;
        public string AppTheme { get => _theme; set => Set(ref _theme, value); }
        private string _theme = "Light";
        public int LogFontSize { get => _logFont; set => Set(ref _logFont, value); }
        private int _logFont = 12;

        public void ApplyFromSettings(GameSettings s)
        {
            if (s == null) return;
            var r = s.Rules; var a = s.Audio; var t = s.Timings; var ap = s.Appearance;

            PlayerCount = r.PlayerCount;
            RoundCount = r.RoundCount;
            FragmentSec = r.FragmentSec;
            DefaultStartSec = r.DefaultStartSec;
            RandomStart = r.RandomStart;
            CountdownOnAnswer = r.CountdownOnAnswer;
            IsSetupMode = r.IsSetupMode;
            AllowRePress = r.AllowRePressAfterWrong;
            ScorePerCorrect = r.ScorePerCorrect;

            Volume = a.Volume;
            BuzzerPath = a.BuzzerPath;
            CorrectPath = a.CorrectPath;
            WrongPath = a.WrongPath;
            TickPath = a.TickPath;
            FadeInMs = a.FadeInMs;
            FadeOutMs = a.FadeOutMs;
            TickLast5 = a.TickLastFiveSeconds;

            CountdownBeforeSec = t.CountdownBeforeSec;
            AnswerTimeoutSec = t.AnswerTimeoutSec;
            PauseAfterPressMs = t.PauseAfterPressMs;
            PauseBetweenRoundsSec = t.PauseBetweenRoundsSec;
            AutoNextRound = t.AutoNextRound;

            PlayerBgColor = ap.PlayerBgColor;
            PlayerAccentColor = ap.PlayerAccentColor;
            PlayerTextColor = ap.PlayerTextColor;
            CategoryFontSize = ap.CategoryFontSize;
            PlayerNameFontSize = ap.PlayerNameFontSize;
            PlayerScoreFontSize = ap.PlayerScoreFontSize;
            AppTheme = ap.AppTheme;
            LogFontSize = ap.LogFontSize;
        }

        public void ApplyToSettings(GameSettings s)
        {
            if (s == null) return;
            var r = s.Rules; var a = s.Audio; var t = s.Timings; var ap = s.Appearance;

            r.PlayerCount = PlayerCount;
            r.RoundCount = RoundCount;
            r.FragmentSec = FragmentSec;
            r.DefaultStartSec = DefaultStartSec;
            r.RandomStart = RandomStart;
            r.CountdownOnAnswer = CountdownOnAnswer;
            r.IsSetupMode = IsSetupMode;
            r.AllowRePressAfterWrong = AllowRePress;
            r.ScorePerCorrect = ScorePerCorrect;

            a.Volume = Volume;
            a.BuzzerPath = BuzzerPath;
            a.CorrectPath = CorrectPath;
            a.WrongPath = WrongPath;
            a.TickPath = TickPath;
            a.FadeInMs = FadeInMs;
            a.FadeOutMs = FadeOutMs;
            a.TickLastFiveSeconds = TickLast5;

            t.CountdownBeforeSec = CountdownBeforeSec;
            t.AnswerTimeoutSec = AnswerTimeoutSec;
            t.PauseAfterPressMs = PauseAfterPressMs;
            t.PauseBetweenRoundsSec = PauseBetweenRoundsSec;
            t.AutoNextRound = AutoNextRound;

            ap.PlayerBgColor = PlayerBgColor;
            ap.PlayerAccentColor = PlayerAccentColor;
            ap.PlayerTextColor = PlayerTextColor;
            ap.CategoryFontSize = CategoryFontSize;
            ap.PlayerNameFontSize = PlayerNameFontSize;
            ap.PlayerScoreFontSize = PlayerScoreFontSize;
            ap.AppTheme = AppTheme;
            ap.LogFontSize = LogFontSize;
        }
    }
}