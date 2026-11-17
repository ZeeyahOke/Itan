using Itan.Detection;
using Itan.Interaction;
using Itan.Narration;
using Itan.Player;
using Itan.Story;
using Itan.Session.States;
using Itan.UI;
using Itan.Voice;
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
        [SerializeField] NarrationManager narration;
        [SerializeField] OpeningScript opening;
        [SerializeField] GreetingDetector greeting;
        [SerializeField] Calabash calabash;
        [SerializeField] MicResponseDetector voice;

        public ISessionState CurrentState { get; private set; }
        public StartMenuUI StartMenu => startMenu;
        public SeatRecenter Seat => seat;
        public NarrationManager Narration => narration;
        public OpeningScript Opening => opening;
        public GreetingDetector Greeting => greeting;
        public Calabash Calabash => calabash;
        public MicResponseDetector Voice => voice;

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
