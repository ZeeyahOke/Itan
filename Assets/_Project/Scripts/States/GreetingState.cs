using UnityEngine;

namespace Itan.Session.States
{
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
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit GreetingState");
        }

        public void Skip()
        {
            session.ChangeState(new CallState(session));
        }
    }
}
