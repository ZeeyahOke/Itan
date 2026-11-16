using UnityEngine;

namespace Itan.Session.States
{
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
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit SummaryState");
        }

        public void Skip()
        {
            session.ChangeState(new MenuState(session));
        }
    }
}
