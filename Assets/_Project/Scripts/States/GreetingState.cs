using UnityEngine;

namespace Itan.Session.States
{
    /// <summary>
    /// The elder welcomes the visitor and asks them to bow. Once they bow,
    /// he hands over the calabash, then the session moves on to the call.
    /// </summary>
    public class GreetingState : ISessionState
    {
        readonly SessionManager session;

        public GreetingState(SessionManager session)
        {
            this.session = session;
        }

        public void Enter()
        {
            Debug.Log("[Session] Enter GreetingState");
            // Face every visitor toward the fire at the same eye height.
            if (session.Seat != null) session.Seat.Recenter();

            session.Greeting.Bowed += OnBowed;
            var o = session.Opening;
            session.Narration.PlaySequence(new[] { o.welcome, o.bowPrompt }, session.Greeting.StartListening);
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit GreetingState");
            session.Greeting.Bowed -= OnBowed;
            session.Greeting.StopListening();
            session.Narration.Stop();
        }

        public void Skip() => GoToCall();

        void OnBowed()
        {
            session.Narration.Play(session.Opening.calabashPrompt, GoToCall);
        }

        void GoToCall()
        {
            session.ChangeState(new CallState(session));
        }
    }
}
