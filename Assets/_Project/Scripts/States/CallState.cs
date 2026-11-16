using UnityEngine;

namespace Itan.Session.States
{
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
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit CallState");
        }

        public void Skip()
        {
            session.ChangeState(new StoryState(session));
        }
    }
}
