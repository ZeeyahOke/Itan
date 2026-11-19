using UnityEngine;

namespace Itan.Story
{
    /// <summary>
    /// What the elder says before the story starts: welcome, the bow,
    /// the calabash and the Àlọ́ o! call.
    /// </summary>
    [CreateAssetMenu(fileName = "Opening", menuName = "Itan/Opening Script")]
    public class OpeningScript : ScriptableObject
    {
        public NarrationLine welcome;
        public NarrationLine bowPrompt;
        public NarrationLine calabashPrompt;

        [Tooltip("The elder explains the call and response before calling.")]
        public NarrationLine callIntro;
        [Tooltip("An example of the answer, so the visitor hears how to say it.")]
        public NarrationLine answerExample;
        public NarrationLine call;
    }
}
