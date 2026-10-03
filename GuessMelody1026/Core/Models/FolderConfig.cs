using System.Collections.Generic;

namespace GuessMelody.Core.Models
{
    public class CategoryConfig
    {
        public string Name { get; set; }  // ќтображаемое им€ (Ђ–окї)
        public string RelativePath { get; set; }  // "rock", путь относительно RootPath
        public List<string> Tracks { get; set; } = new List<string>();
    }

    public class FolderConfig
    {
        public int SchemaVersion { get; set; } = 1;
        public string RootPath { get; set; } = "";
        public double DefaultPreviewStartSec { get; set; } = 0.0;
        public List<CategoryConfig> Categories { get; set; } = new List<CategoryConfig>();

        //  люч Ч "<relativePath>/<fileName>", значение Ч с какой секунды превью
        public Dictionary<string, double> PreviewStartSec { get; set; }
            = new Dictionary<string, double>();
    }
}