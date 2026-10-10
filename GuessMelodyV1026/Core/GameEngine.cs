using GuessMelody.Core.Models;
using GuessMelody.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuessMelody.Core
{
    public class PressEvent
    {
        public string Mac { get; set; }
        public string TeamName { get; set; }
        public string Seq { get; set; }
        public long UptimeMs { get; set; }
        public DateTime ReceivedUtc { get; set; }
    }

    public class GameEngine
    {
        private readonly LogService _log;
        private readonly List<PressEvent> _pressQueue = new List<PressEvent>();
        private readonly HashSet<string> _blockedThisRound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public GameSettings Settings { get; private set; } = new GameSettings();
        public RoundState State { get; private set; } = RoundState.Idle;
        public int CurrentRound { get; private set; }
        public int TotalRounds => Settings?.Rules?.RoundCount ?? 0;
        public Category CurrentCategory { get; private set; }
        public Track CurrentTrack { get; private set; }
        public Team FirstPressedTeam { get; private set; }
        public IReadOnlyList<PressEvent> PressQueue => _pressQueue;
        public IReadOnlyList<Team> Teams => Settings?.Buttons?.Teams ?? new List<Team>();

        // ---- События ----
        public event EventHandler StateChanged;
        public event EventHandler ScoreChanged;
        public event EventHandler<Track> TrackChanged;

        public GameEngine(LogService log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void ApplySettings(GameSettings settings)
        {
            Settings = settings ?? new GameSettings();
            _log.Add(LogKind.Game, "Настройки применены");
        }

        // ---------- Заглушки, полная логика будет на Этапе 11 ----------
        public void StartRound(int categoryIndex)
        {
            _log.Add(LogKind.Game, $"[TODO] StartRound({categoryIndex})");
            State = RoundState.Idle;
            RaiseState();
        }

        public void OnPlayerPress(string mac, DateTime receivedUtc, string seq, long uptimeMs)
        {
            _log.Add(LogKind.Press, $"[TODO] Press {mac}");
            // Заглушка — реальная логика на этапе 11
        }

        public void SubmitScore(bool correct)
        {
            _log.Add(LogKind.Game, $"[TODO] SubmitScore({correct})");
        }

        public void AdjustScore(string mac, int delta)
        {
            var team = Teams.FirstOrDefault(t =>
                string.Equals(t.Mac, mac, StringComparison.OrdinalIgnoreCase));
            if (team == null) return;
            team.Score += delta;
            if (team.Score < 0) team.Score = 0;
            _log.Add(LogKind.Score, $"{team.Name} {delta:+#;-#;0} → {team.Score}");
            ScoreChanged?.Invoke(this, EventArgs.Empty);
        }

        public void PauseMusic() { }
        public void ResumeMusic() { }
        public void NextRound() { }
        public void Panic() { }

        private void RaiseState() => StateChanged?.Invoke(this, EventArgs.Empty);
    }
}