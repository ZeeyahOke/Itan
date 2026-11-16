using UnityEngine;

namespace Itan.Session.States
{
    public class QuizState : ISessionState
    {
        readonly SessionManager session;

        public QuizState(SessionManager session)
        {
            this.session = session;
        }

        public void Enter()
        {
            Debug.Log("[Session] Enter QuizState");
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit QuizState");
        }

        public void Skip()
        {
            session.ChangeState(new SummaryState(session));
        }
    }
}
