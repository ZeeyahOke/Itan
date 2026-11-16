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
    }
}
