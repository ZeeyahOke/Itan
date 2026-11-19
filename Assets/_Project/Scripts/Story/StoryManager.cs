using System;
using System.Collections;
using System.Collections.Generic;
using Itan.Narration;
using UnityEngine;

namespace Itan.Story
{
    /// <summary>
    /// Plays the story beats in order, straight from the StoryBeat data.
    /// For each beat: the elder narrates its lines, then (later tasks) its
    /// key moment plays and its word card appears.
    /// </summary>
    public class StoryManager : MonoBehaviour
    {
        [SerializeField] NarrationManager narration;
        [SerializeField] List<StoryBeat> beats = new List<StoryBeat>();
        [SerializeField] WordCardMoment wordMoment;

        [Tooltip("One key moment per beat, in beat order. Each must implement IKeyMoment.")]
        [SerializeField] MonoBehaviour[] keyMoments = new MonoBehaviour[0];

        /// <summary>A new beat is starting. Arguments: the beat and its 0-based index.</summary>
        public event Action<StoryBeat, int> BeatStarted;

        public IReadOnlyList<StoryBeat> Beats => beats;
        public WordCardMoment WordMoment => wordMoment;

        Coroutine running;

        void Awake()
        {
            // Play in beat-number order, whatever order they were dragged in.
            beats.Sort((a, b) => a.beatNumber.CompareTo(b.beatNumber));
        }

        public void PlayAll(Action onDone)
        {
            Stop();
            running = StartCoroutine(Run(onDone));
        }

        public void Stop()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            narration.Stop();
            if (wordMoment != null) wordMoment.Cancel();
        }

        IEnumerator Run(Action onDone)
        {
            for (int i = 0; i < beats.Count; i++)
            {
                var beat = beats[i];
                Debug.Log("[Story] Beat " + beat.beatNumber + ": " + beat.title);
                BeatStarted?.Invoke(beat, i);

                yield return Narrate(beat.narration);

                if (i < keyMoments.Length && keyMoments[i] is IKeyMoment moment)
                    yield return moment.Play();

                if (wordMoment != null && beat.wordCard != null)
                    yield return wordMoment.Play(beat.wordCard);
            }

            running = null;
            Debug.Log("[Story] All beats played");
            onDone?.Invoke();
        }

        /// <summary>Put the rope and characters back for the next visitor.</summary>
        public void ResetMoments()
        {
            foreach (var m in keyMoments)
                if (m is IKeyMoment moment) moment.ResetMoment();
        }

        IEnumerator Narrate(IList<NarrationLine> lines)
        {
            bool done = false;
            narration.PlaySequence(lines, () => done = true);
            yield return new WaitUntil(() => done);
        }
    }
}
