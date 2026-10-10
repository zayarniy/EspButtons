using System.Windows.Input;
using GuessMelody.Services;

namespace GuessMelody.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public GameTabViewModel Game { get; }
        public SettingsTabViewModel Settings { get; }
        public ButtonsTabViewModel Buttons { get; }
        public FoldersTabViewModel Folders { get; }
        public LogTabViewModel Log { get; }
        public AnswerServerTabViewModel AnswerServer { get; }

        public ICommand ExitCommand { get; }

        public MainViewModel(
            GameTabViewModel game,
            SettingsTabViewModel settings,
            ButtonsTabViewModel buttons,
            FoldersTabViewModel folders,
            LogTabViewModel log,
            AnswerServerTabViewModel answerServer)
        {
            Game = game;
            Settings = settings;
            Buttons = buttons;
            Folders = folders;
            Log = log;
            AnswerServer = answerServer;

            Game.Buttons = Buttons;
            Game.Settings = Settings;
            Game.Folders = Folders;

            ExitCommand = new RelayCommand(_ =>
                System.Windows.Application.Current.Shutdown());
        }

        private string _statusText = "Готово";
        public string StatusText
        {
            get => _statusText;
            set => Set(ref _statusText, value);
        }
    }
}