using UnityEngine;

namespace Itan.Session.States
{
    /// <summary>
    /// Smoke carries the visitor into the story world, the beats play,
    /// then smoke carries them back to the fire for the quiz.
    /// </summary>
    public class StoryState : ISessionState
    {
        readonly SessionManager session;

        public StoryState(SessionManager session)
        {
            this.session = session;
        }

        // Callbacks from the transition or story can arrive after a skip; ignore them then.
        bool IsCurrent => session.CurrentState == this;

        public void Enter()
        {
            Debug.Log("[Session] Enter StoryState");
            session.Story.WordMoment.WordCollected += RecordWord;
            session.Transition.SmokeToStory(() =>
            {
                if (IsCurrent) session.Story.PlayAll(OnStoryDone);
            });
        }

        public void Tick() { }

        public void Exit()
        {
            Debug.Log("[Session] Exit StoryState");
            session.Story.WordMoment.WordCollected -= RecordWord;
            session.Story.Stop();
        }

        public void Skip()
        {
            session.Transition.Cancel();
            session.Transition.ShowWorld(false);
            GoToQuiz();
        }

        void RecordWord(Itan.Story.WordCard word, int tries, bool byButton)
        {
            session.Results.words.Add(new WordResult { word = word.yorubaWord, tries = tries, answeredByButton = byButton });
        }

        void OnStoryDone()
        {
            if (!IsCurrent) return;
            session.Transition.SmokeToFire(() =>
            {
                if (IsCurrent) GoToQuiz();
            });
        }

        void GoToQuiz()
        {
            session.ChangeState(new QuizState(session));
        }
    }
}
