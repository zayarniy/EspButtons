using GuessMelody.Core.Models;

namespace GuessMelody.ViewModels
{
    public class FoldersTabViewModel : ViewModelBase
    {
        private string _rootFolder = "";
        public string RootFolder
        {
            get => _rootFolder;
            set => Set(ref _rootFolder, value);
        }

        private string _presetName = "default";
        public string PresetName
        {
            get => _presetName;
            set => Set(ref _presetName, value);
        }

        public void ApplyFromSettings(GameSettings s)
        {
            if (s?.Folders == null) return;
            RootFolder = s.Folders.RootFolder ?? "";
            PresetName = s.Folders.PresetName ?? "default";
        }

        public void ApplyToSettings(GameSettings s)
        {
            if (s == null) return;
            if (s.Folders == null) s.Folders = new FoldersCatalog();
            s.Folders.RootFolder = RootFolder;
            s.Folders.PresetName = PresetName;
        }
    }
}