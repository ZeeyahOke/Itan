using UnityEngine;

namespace Itan.Session.States
{
    /// <summary>
    /// Back at the fire, the elder checks what the visitor remembers with a
    /// three-round card quiz, then the session moves on to the summary.
    /// </summary>
    public class QuizState : ISessionState
    {
        readonly SessionManager session;

        public QuizState(SessionManager session)
        {
            this.session = session;
        }

        bool IsCurrent => session.CurrentState == this;

        public void Enter()
        {
            Debug.Log("[Session] Enter QuizState");
            session.Narration.Play(session.Closing.quizIntro, () =>
            {
                if (IsCurrent) session.Quiz.Run(OnQuizDone);
            });
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit QuizState");
            session.Quiz.Stop();
            session.Narration.Stop();
        }

        public void Skip() => GoToSummary();

        void OnQuizDone(int score, int total)
        {
            session.Results.quizScore = score;
            session.Results.quizTotal = total;
            Debug.Log("[Quiz] Finished: " + score + " of " + total);
            GoToSummary();
        }

        void GoToSummary()
        {
            session.ChangeState(new SummaryState(session));
        }
    }
}
