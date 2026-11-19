using UnityEngine;

namespace Itan.Story
{
    /// <summary>
    /// What the elder says after the story: the quiz introduction, the
    /// lesson of the tale and his goodbye.
    /// </summary>
    [CreateAssetMenu(fileName = "Closing", menuName = "Itan/Closing Script")]
    public class ClosingScript : ScriptableObject
    {
        public NarrationLine quizIntro;
        public NarrationLine lesson;
        public NarrationLine farewell;
    }
}
