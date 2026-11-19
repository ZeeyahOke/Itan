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
        [SerializeField] SummaryPanel summary;
        [SerializeField] SessionLogger logger;

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
        public SummaryPanel Summary => summary;
        public SessionLogger Logger => logger;

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

        /// <summary>
        /// Clear everything one visitor did (calabash, cards, score, story world)
        /// and go back to the start menu, without restarting the app.
        /// </summary>
        public void ResetForNextVisitor()
        {
            Debug.Log("[Session] Reset for next visitor");
            narration.Stop();
            voice.Cancel();
            story.Stop();
            story.ResetMoments();
            quiz.ResetQuiz();
            status.ResetStatus();
            calabash.ResetForNextVisitor();
            transition.Cancel();
            transition.ShowWorld(false);
            Results = new SessionResults();
            ChangeState(new MenuState(this));
        }

        public void ChangeState(ISessionState next)
        {
            CurrentState?.Exit();
            CurrentState = next;
            CurrentState.Enter();
        }
    }
}
