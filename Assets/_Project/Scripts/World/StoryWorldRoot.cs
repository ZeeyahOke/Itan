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

        [Tooltip("Everything you see in the story. Keep it active in the scene so static batching can combine it; it is hidden here on load and shown by the smoke transition.")]
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
#if UNITY_EDITOR
            // Builds combine the static scenery at build time; the editor doesn't for this additive scene.
            StaticBatchingUtility.Combine(content);
#endif
            content.SetActive(false);
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }
    }
}
