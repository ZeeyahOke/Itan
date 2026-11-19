using System;
using System.Collections;
using Itan.Narration;
using Itan.UI;
using Itan.Voice;
using UnityEngine;

namespace Itan.Story
{
    /// <summary>
    /// The pause at the end of each beat: the card appears, the elder says the
    /// word, the visitor repeats it (voice check, button after two misses),
    /// then the card glows, a sting plays and the card flies into the calabash.
    /// </summary>
    public class WordCardMoment : MonoBehaviour
    {
        [SerializeField] WordCardView card;
        [SerializeField] NarrationManager narration;
        [SerializeField] MicResponseDetector voice;
        [SerializeField] Transform calabash;
        [SerializeField] AudioSource sting;

        [SerializeField] float glowSeconds = 0.6f;
        [SerializeField] float flySeconds = 0.9f;
        [SerializeField] float arcHeight = 0.25f;

        /// <summary>The word is in the calabash. Arguments: the word, listening tries, answered by button.</summary>
        public event Action<WordCard, int, bool> WordCollected;

        NarrationLine currentLine;

        public IEnumerator Play(WordCard word)
        {
            card.Show(word);
            currentLine = new NarrationLine
            {
                clip = word.elderClip,
                subtitle = word.yorubaWord + "  (" + word.englishMeaning + ").  Say it with me!"
            };

            // Elder says the word.
            yield return Say();

            // Visitor repeats it. On a miss the elder says it again, then we listen again.
            bool answered = false;
            voice.Missed += OnMissed;
            voice.Listen(() => answered = true);
            yield return new WaitUntil(() => answered);
            voice.Missed -= OnMissed;

            int tries = voice.LastTries;
            bool byButton = voice.LastAnsweredByButton;
            Debug.Log("[Word] " + word.yorubaWord + " repeated (tries " + tries + (byButton ? ", button" : "") + ")");

            // Glow and sting.
            if (sting != null) sting.Play();
            for (float t = 0f; t < glowSeconds; t += Time.deltaTime)
            {
                card.SetGlow(Mathf.Sin(t / glowSeconds * Mathf.PI));
                yield return null;
            }

            // Fly in an arc into the calabash, shrinking as it goes.
            Vector3 start = card.transform.position;
            Vector3 startScale = card.HomeScale;
            for (float t = 0f; t < flySeconds; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / flySeconds);
                Vector3 p = Vector3.Lerp(start, calabash.position, k);
                p.y += Mathf.Sin(k * Mathf.PI) * arcHeight;
                card.transform.position = p;
                card.transform.localScale = Vector3.Lerp(startScale, startScale * 0.05f, k);
                yield return null;
            }

            card.Hide();
            WordCollected?.Invoke(word, tries, byButton);
        }

        public void Cancel()
        {
            voice.Missed -= OnMissed;
            voice.Cancel();
            card.Hide();
        }

        IEnumerator Say()
        {
            bool done = false;
            narration.Play(currentLine, () => done = true);
            yield return new WaitUntil(() => done);
        }

        void OnMissed(int misses)
        {
            narration.Play(currentLine, voice.ListenAgain);
        }
    }
}
