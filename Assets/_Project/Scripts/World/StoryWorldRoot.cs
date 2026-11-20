using System.Collections.Generic;
using UnityEngine;

namespace Itan.World
{
    /// <summary>
    /// Lives at the top of the StoryWorld scene. The hub scene can't hold references
    /// into another scene, so the hub finds the story world through this once the
    /// scene has loaded: what to show and hide, and the key moments in beat order.
    /// </summary>
    public class StoryWorldRoot : MonoBehaviour
    {
        /// <summary>The loaded story world, or null if its scene isn't loaded yet.</summary>
        public static StoryWorldRoot Current { get; private set; }

        [Tooltip("Everything you see in the story. Starts hidden; the smoke transition shows it.")]
        [SerializeField] GameObject content;

        [Tooltip("One key moment per beat, in beat order. Each must implement IKeyMoment.")]
        [SerializeField] MonoBehaviour[] keyMoments = new MonoBehaviour[0];

        [Tooltip("Optional, per beat: plays before the beat's narration (e.g. returning the visitor to the ground).")]
        [SerializeField] MonoBehaviour[] beatIntros = new MonoBehaviour[0];

        public IReadOnlyList<MonoBehaviour> KeyMoments => keyMoments;
        public IReadOnlyList<MonoBehaviour> BeatIntros => beatIntros;

        public void Show(bool visible) => content.SetActive(visible);

        void Awake()
        {
            Current = this;
            content.SetActive(false);
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }
    }
}
