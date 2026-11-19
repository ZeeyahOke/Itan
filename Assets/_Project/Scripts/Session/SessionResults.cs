using System;
using System.Collections.Generic;

namespace Itan.Session
{
    /// <summary>
    /// What happened in one visit, filled in as the session runs and saved as
    /// JSON at the end. No names, voices or other personal data, only timings,
    /// tries and the quiz score.
    /// </summary>
    [Serializable]
    public class SessionResults
    {
        public string startedAt;              // local date and time the visit began
        public float sessionSeconds;          // from Begin to the summary
        public float secondsToAnswerCall;     // from the Àlọ́ o! call to the answer
        public bool callAnsweredByButton;
        public List<WordResult> words = new List<WordResult>();
        public int quizScore;
        public int quizTotal;

        [NonSerialized] public float startTime;
    }

    [Serializable]
    public class WordResult
    {
        public string word;
        public int tries;
        public bool answeredByButton;
    }
}
