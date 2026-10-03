using System;

namespace GuessMelody.Core.Models
{
    public class RoundRecord
    {
        public int RoundNumber { get; set; }
        public string Category { get; set; }
        public string TrackFile { get; set; }
        public string WinnerMac { get; set; }
        public string WinnerName { get; set; }
        public EndReason Reason { get; set; }
        public double AnswerTimeSec { get; set; }  // сколько секунд от паузы до «Да/Нет»

        public DateTime StartedUtc { get; set; }
        public DateTime FinishedUtc { get; set; }
    }
}