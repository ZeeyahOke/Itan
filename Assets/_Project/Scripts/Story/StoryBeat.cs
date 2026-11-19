using System.Collections.Generic;
using UnityEngine;

namespace Itan.Story
{
    /// <summary>
    /// One part of the story. The elder narrates the lines in order,
    /// then the beat pauses for its word card.
    /// </summary>
    [CreateAssetMenu(fileName = "Beat_", menuName = "Itan/Story Beat")]
    public class StoryBeat : ScriptableObject
    {
        [Tooltip("1, 2 or 3. Used for ordering and the status label.")]
        public int beatNumber = 1;

        [Tooltip("Short title shown in the status label, e.g. The Famine.")]
        public string title;

        [Tooltip("What the elder says in this beat, in order.")]
        public List<NarrationLine> narration = new List<NarrationLine>();

        [Tooltip("What the elder says after the beat's key moment (e.g. explaining the fall).")]
        public List<NarrationLine> afterMoment = new List<NarrationLine>();

        [Tooltip("The word the player learns at the end of this beat.")]
        public WordCard wordCard;
    }
}
