using UnityEngine;

namespace Itan.Session.States
{
    /// <summary>
    /// The elder welcomes the visitor and asks them to bow. Once they bow,
    /// he offers the calabash; when it is in their lap, the session moves on to the call.
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
            session.Results.startTime = Time.realtimeSinceStartup;
            session.Results.startedAt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // Face every visitor toward the fire at the same eye height.
            if (session.Seat != null) session.Seat.Recenter();

            session.Greeting.Bowed += OnBowed;
            session.Calabash.Snapped += GoToCall;
            var o = session.Opening;
            session.Narration.PlaySequence(new[] { o.welcome, o.bowPrompt }, session.Greeting.StartListening);
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit GreetingState");
            session.Greeting.Bowed -= OnBowed;
            session.Calabash.Snapped -= GoToCall;
            session.Greeting.StopListening();
            session.Narration.Stop();
        }

        public void Skip()
        {
            // Skipping still leaves the calabash in the lap, so later steps can rely on it.
            if (!session.Calabash.IsInLap) session.Calabash.SnapToLap(); // fires Snapped → GoToCall
            else GoToCall();
        }

        void OnBowed()
        {
            session.Calabash.Offer();
            session.Narration.Play(session.Opening.calabashPrompt);
        }

        void GoToCall()
        {
            session.ChangeState(new CallState(session));
        }
    }
}
