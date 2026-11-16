using UnityEngine;

namespace Itan.Session.States
{
    public class StoryState : ISessionState
    {
        readonly SessionManager session;

        public StoryState(SessionManager session)
        {
            this.session = session;
        }

        public void Enter()
        {
            Debug.Log("[Session] Enter StoryState");
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit StoryState");
        }

        public void Skip()
        {
            session.ChangeState(new QuizState(session));
        }
    }
}
