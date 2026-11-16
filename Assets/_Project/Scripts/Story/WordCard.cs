using UnityEngine;

namespace Itan.Story
{
    /// <summary>
    /// One Yoruba word the player learns. Shown as a card during a story beat,
    /// collected into the calabash, and used again in the quiz.
    /// </summary>
    [CreateAssetMenu(fileName = "Word_", menuName = "Itan/Word Card")]
    public class WordCard : ScriptableObject
    {
        [Tooltip("The word in Yoruba, with tone marks, e.g. ajá.")]
        public string yorubaWord;

        [Tooltip("Plain English meaning shown on the card and in the quiz, e.g. dog.")]
        public string englishMeaning;

        [Tooltip("Simple pronunciation guide shown under the word, e.g. ah-JAH.")]
        public string pronunciation;

        [Tooltip("The elder saying the word, played before the player repeats it.")]
        public AudioClip elderClip;

        [Tooltip("Picture on the card. Optional for the greybox.")]
        public Sprite picture;
    }
}
