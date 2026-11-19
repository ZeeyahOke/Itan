using System;
using System.Collections;
using System.Collections.Generic;
using Itan.Narration;
using Itan.Story;
using Itan.UI;
using UnityEngine;

namespace Itan.Quiz
{
    /// <summary>
    /// The multiple-choice quiz: three rounds. Each round the elder says a
    /// word, three meaning cards fan out, and the visitor picks one with the ray.
    /// </summary>
    public class CardQuiz : MonoBehaviour
    {
        [SerializeField] StoryManager story;
        [SerializeField] NarrationManager narration;
        [SerializeField] QuizCard[] cards;
        [SerializeField] float resultPause = 1.4f;

        [Header("Sound")]
        [SerializeField] AudioSource sfx;
        [SerializeField] AudioClip rightSting;
        [SerializeField] AudioClip wrongSting;

        /// <summary>A round was answered. Arguments: the word asked, whether the pick was right.</summary>
        public event Action<WordCard, bool> RoundAnswered;

        readonly List<WordCard> words = new List<WordCard>();
        QuizLogic logic;
        Coroutine running;
        QuizCard picked;

        void Awake()
        {
            foreach (var beat in story.Beats) if (beat.wordCard != null) words.Add(beat.wordCard);
            foreach (var card in cards) card.Picked += c => picked = c;
            HideCards();
        }

        public int Total => words.Count;

        public void Run(Action<int, int> onDone)
        {
            Stop();
            running = StartCoroutine(Play(onDone));
        }

        public void Stop()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            narration.Stop();
            HideCards();
        }

        public void ResetQuiz() => Stop();

        IEnumerator Play(Action<int, int> onDone)
        {
            logic = new QuizLogic(words.Count, cards.Length, new System.Random());

            while (!logic.IsFinished)
            {
                var word = words[logic.CurrentWord];

                // Elder asks.
                bool asked = false;
                narration.Play(new NarrationLine
                {
                    clip = word.elderClip,
                    subtitle = "Which card means " + word.yorubaWord + "?"
                }, () => asked = true);
                yield return new WaitUntil(() => asked);

                // Cards fan out.
                int[] options = logic.OptionsForRound();
                for (int i = 0; i < cards.Length; i++)
                {
                    if (i < options.Length) cards[i].Show(words[options[i]].englishMeaning, options[i]);
                    else cards[i].Hide();
                }

                // Visitor picks.
                picked = null;
                yield return new WaitUntil(() => picked != null);
                foreach (var card in cards) card.SetInteractable(false);

                bool correct = logic.Answer(picked.WordIndex);
                picked.Flash(correct);
                if (sfx != null) sfx.PlayOneShot(correct ? rightSting : wrongSting);
                if (!correct)
                    foreach (var card in cards)
                        if (card.isActiveAndEnabled && card.WordIndex == words.IndexOf(word)) card.Flash(true);

                Debug.Log("[Quiz] " + word.yorubaWord + ": " + (correct ? "right" : "wrong") + " (score " + logic.Score + ")");
                RoundAnswered?.Invoke(word, correct);

                yield return new WaitForSeconds(resultPause);
                HideCards();
            }

            running = null;
            onDone?.Invoke(logic.Score, logic.Rounds);
        }

        void HideCards()
        {
            foreach (var card in cards) if (card != null) card.Hide();
        }
    }
}
