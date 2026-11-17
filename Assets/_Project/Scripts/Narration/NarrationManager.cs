using System;
using System.Collections;
using System.Collections.Generic;
using Itan.Story;
using Itan.UI;
using UnityEngine;

namespace Itan.Narration
{
    /// <summary>
    /// Plays the elder's lines from his spatial voice source and shows the
    /// English subtitle for each line while it plays.
    /// </summary>
    public class NarrationManager : MonoBehaviour
    {
        [Tooltip("3D AudioSource at the elder's head, so the voice comes from him.")]
        [SerializeField] AudioSource voice;
        [SerializeField] SubtitlePanel subtitles;

        [Tooltip("Pause after each line before the next one starts.")]
        [SerializeField] float gapAfterLine = 0.4f;

        [Tooltip("How long a line with no audio stays on screen.")]
        [SerializeField] float silentLineSeconds = 2.5f;

        /// <summary>True while the elder is talking. Used to switch his animation.</summary>
        public event Action<bool> SpeakingChanged;
        public bool IsSpeaking { get; private set; }

        Coroutine running;

        public void Play(NarrationLine line, Action onDone = null)
        {
            PlaySequence(new[] { line }, onDone);
        }

        public void PlaySequence(IList<NarrationLine> lines, Action onDone = null)
        {
            Stop();
            running = StartCoroutine(Run(lines, onDone));
        }

        public void Stop()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            voice.Stop();
            subtitles.Hide();
            SetSpeaking(false);
        }

        IEnumerator Run(IList<NarrationLine> lines, Action onDone)
        {
            SetSpeaking(true);
            foreach (var line in lines)
            {
                if (line == null) continue;
                subtitles.Show(line.subtitle);

                float length = silentLineSeconds;
                if (line.clip != null)
                {
                    voice.clip = line.clip;
                    voice.Play();
                    length = line.clip.length;
                }
                yield return new WaitForSeconds(length + gapAfterLine);
            }

            subtitles.Hide();
            SetSpeaking(false);
            running = null;
            onDone?.Invoke();
        }

        void SetSpeaking(bool speaking)
        {
            if (IsSpeaking == speaking) return;
            IsSpeaking = speaking;
            SpeakingChanged?.Invoke(speaking);
        }
    }
}
