using System.Collections;

namespace Itan.Story
{
    /// <summary>
    /// The big visual moment of a story beat (the rope drops, Ajá climbs,
    /// Ìjàpá falls). Plays after the beat's narration, before its word card.
    /// </summary>
    public interface IKeyMoment
    {
        /// <summary>Play the moment; the story waits until it finishes.</summary>
        IEnumerator Play();

        /// <summary>Put everything back as it was before the story, for the next visitor.</summary>
        void ResetMoment();
    }
}
