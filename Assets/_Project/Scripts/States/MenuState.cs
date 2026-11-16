using UnityEngine;

namespace Itan.Session.States
{
    public class MenuState : ISessionState
    {
        readonly SessionManager session;

        public MenuState(SessionManager session)
        {
            this.session = session;
        }

        public void Enter()
        {
            Debug.Log("[Session] Enter MenuState");
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit MenuState");
        }

        public void Skip()
        {
            session.ChangeState(new GreetingState(session));
        }
    }
}
