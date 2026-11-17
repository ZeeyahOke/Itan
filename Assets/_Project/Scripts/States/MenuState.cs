using UnityEngine;

namespace Itan.Session.States
{
    /// <summary>
    /// Shows the title screen and waits for the visitor to press Begin.
    /// </summary>
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
            // Face every visitor toward the fire at the same eye height.
            if (session.Seat != null) session.Seat.Recenter();
            if (session.StartMenu != null)
            {
                session.StartMenu.Show();
                session.StartMenu.BeginPressed += OnBegin;
            }
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit MenuState");
            if (session.StartMenu != null)
            {
                session.StartMenu.BeginPressed -= OnBegin;
                session.StartMenu.Hide();
            }
        }

        public void Skip() => OnBegin();

        void OnBegin()
        {
            session.ChangeState(new GreetingState(session));
        }
    }
}
