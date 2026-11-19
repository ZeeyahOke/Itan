using System;
using UnityEngine;

namespace Itan.Story
{
    /// <summary>
    /// One spoken line from the elder: the audio and its English subtitle.
    /// </summary>
    [Serializable]
    public class NarrationLine
    {
        public AudioClip clip;

        [TextArea(2, 4)]
        public string subtitle;

        [Tooltip("What the narrator voice actually reads, with Yoruba names spelled out for the English voice " +
                 "(e.g. Ah-jah for Ajá). Leave empty to read the subtitle. Only used when generating narration.")]
        [TextArea(2, 4)]
        public string spokenText;
    }
}
