using System;

using System.Collections.Generic;

namespace GuessMelody.Core.Models
{
    public class GameSettings
    {
        public int SchemaVersion { get; set; } = 1;

        // --- Раунд ---
        public int PlayDurationSec { get; set; } = 30;   // сколько играет до авто-паузы
        public StartAtMode StartAtMode { get; set; } = StartAtMode.FromRandomPlace;
        public double StartAtSec { get; set; } = 15.0; // если StartAtMode == FromSpecificTime

        // --- Звук старта раунда ---
        public bool PlayRoundStartSound { get; set; } = true;
        public string RoundStartSoundFile { get; set; } = "sounds/round_start.wav";

        // --- Обратный отсчёт перед раундом ---
        public bool PlayCountdown { get; set; } = true;
        public int CountdownSeconds { get; set; } = 3;
        public string CountdownTickSoundFile { get; set; } = "sounds/tick.wav";
        public string CountdownEndSoundFile { get; set; } = "sounds/go.wav";

        // --- Обратный отсчёт на ответ ---
        public int AnswerSeconds { get; set; } = 10;
        public string AnswerTickSoundFile { get; set; } = "sounds/tick.wav";

        // --- Звуки результата ---
        public string RightSoundFile { get; set; } = "sounds/right.wav";
        public string WrongSoundFile { get; set; } = "sounds/wrong.wav";

        // --- Раунд ---
        public bool AutoNextRound { get; set; } = false;
        public int MaxRounds { get; set; } = 0;    // 0 = без ограничения

        // --- Раскладка игрового экрана ---
        public int GridColumns { get; set; } = 3;
        public int GridRows { get; set; } = 3;
    }
}