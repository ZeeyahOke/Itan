using System.Collections.Generic;
using Itan.Story;
using UnityEngine;

namespace Itan.Session.States
{
    /// <summary>
    /// The elder shares the lesson of the tale and says goodbye while the
    /// visitor sees the words they learned and their quiz score. Finish
    /// resets everything and returns to the start menu for the next visitor.
    /// </summary>
    public class SummaryState : ISessionState
    {
        readonly SessionManager session;

        public SummaryState(SessionManager session)
        {
            this.session = session;
        }

        public void Enter()
        {
            Debug.Log("[Session] Enter SummaryState");

            var words = new List<WordCard>();
            foreach (var beat in session.Story.Beats) if (beat.wordCard != null) words.Add(beat.wordCard);

            session.Summary.Show(words, session.Results.QuizScore, session.Results.QuizTotal);
            session.Summary.FinishPressed += Finish;

            var c = session.Closing;
            session.Narration.PlaySequence(new[] { c.lesson, c.farewell });
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit SummaryState");
            session.Summary.FinishPressed -= Finish;
            session.Summary.Hide();
            session.Narration.Stop();
        }

        public void Skip() => Finish();

        void Finish() => session.ResetForNextVisitor();
    }
}
