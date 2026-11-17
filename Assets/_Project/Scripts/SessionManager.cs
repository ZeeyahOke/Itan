using Itan.Player;
using Itan.Session.States;
using Itan.UI;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace Itan.Session
{
    /// <summary>
    /// Runs one visit as a chain of states:
    /// Menu → Greeting → Call → Story → Quiz → Summary → back to Menu.
    /// Each state decides which state comes after it.
    /// </summary>
    public class SessionManager : MonoBehaviour
    {
        [SerializeField] StartMenuUI startMenu;
        [SerializeField] SeatRecenter seat;

        public ISessionState CurrentState { get; private set; }
        public StartMenuUI StartMenu => startMenu;
        public SeatRecenter Seat => seat;

        void Start()
        {
            ChangeState(new MenuState(this));
        }

        void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Debug: press N to skip to the next state.
            if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
                CurrentState?.Skip();
#endif
            CurrentState?.Tick();
        }

        public void ChangeState(ISessionState next)
        {
            CurrentState?.Exit();
            CurrentState = next;
            CurrentState.Enter();
        }
    }
}
