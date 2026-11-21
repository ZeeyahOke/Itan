using Itan.Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Itan.UI
{
    /// <summary>
    /// The floating word card: the Yoruba word, how to say it, and what it means,
    /// in an adire frame, with a glow behind it for the moment it is learned.
    /// </summary>
    public class WordCardView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI yoruba;
        [SerializeField] TextMeshProUGUI pronunciation;
        [SerializeField] TextMeshProUGUI meaning;
        [SerializeField] Image glow;

        Vector3 homePosition;
        Quaternion homeRotation;
        Vector3 homeScale;

        void Awake()
        {
            // Local, so the card comes up in front of the visitor wherever they are (see VisitorSpace).
            homePosition = transform.localPosition;
            homeRotation = transform.localRotation;
            homeScale = transform.localScale;
            Hide();
        }

        public void Show(WordCard card)
        {
            yoruba.text = card.yorubaWord;
            pronunciation.text = card.pronunciation;
            meaning.text = card.englishMeaning;
            SetGlow(0f);
            transform.SetLocalPositionAndRotation(homePosition, homeRotation);
            transform.localScale = homeScale;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        public void SetGlow(float amount)
        {
            var c = glow.color;
            c.a = Mathf.Clamp01(amount);
            glow.color = c;
        }

        public Vector3 HomeScale => homeScale;
    }
}
