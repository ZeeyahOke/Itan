using Itan.Detection;
using Itan.Interaction;
using Itan.Narration;
using Itan.Player;
using Itan.Quiz;
using Itan.Story;
using Itan.Session.States;
using Itan.UI;
using Itan.Voice;
using Itan.World;
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
        [SerializeField] SceneTransition transition;
        [SerializeField] StoryManager story;
        [SerializeField] StatusUI status;
        [SerializeField] CardQuiz quiz;
        [SerializeField] ClosingScript closing;

        public ISessionState CurrentState { get; private set; }
        public StartMenuUI StartMenu => startMenu;
        public SeatRecenter Seat => seat;
        public NarrationManager Narration => narration;
        public OpeningScript Opening => opening;
        public GreetingDetector Greeting => greeting;
        public Calabash Calabash => calabash;
        public MicResponseDetector Voice => voice;
        public SceneTransition Transition => transition;
        public StoryManager Story => story;
        public StatusUI Status => status;
        public CardQuiz Quiz => quiz;
        public ClosingScript Closing => closing;

        /// <summary>What happened this visit (quiz score, and more later). Fresh for every visitor.</summary>
        public SessionResults Results { get; private set; } = new SessionResults();

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
