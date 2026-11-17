using UnityEngine;

namespace Itan.Session.States
{
    /// <summary>
    /// The elder opens the storytelling with the traditional call "Àlọ́ o!" and
    /// waits for the visitor to answer out loud. If they stay quiet he calls
    /// again; after two misses a button lets them answer instead.
    /// </summary>
    public class CallState : ISessionState
    {
        readonly SessionManager session;

        public CallState(SessionManager session)
        {
            this.session = session;
        }

        public void Enter()
        {
            Debug.Log("[Session] Enter CallState");
            session.Voice.Missed += OnMissed;
            session.Narration.Play(session.Opening.call, ListenForAnswer);
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit CallState");
            session.Voice.Missed -= OnMissed;
            session.Voice.Cancel();
            session.Narration.Stop();
        }

        public void Skip() => OnAnswered();

        void ListenForAnswer() => session.Voice.Listen(OnAnswered);

        void OnMissed(int misses)
        {
            // Call again, then listen again.
            session.Narration.Play(session.Opening.call, session.Voice.ListenAgain);
        }

        void OnAnswered()
        {
            Debug.Log("[Call] Visitor answered the call");
            session.ChangeState(new StoryState(session));
        }
    }
}
