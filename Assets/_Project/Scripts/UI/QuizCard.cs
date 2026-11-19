using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Itan.UI
{
    /// <summary>
    /// One selectable quiz card showing a word's English meaning. The visitor
    /// picks it with the controller ray; it then flashes green (right) or red (wrong).
    /// </summary>
    public class QuizCard : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI meaning;
        [SerializeField] Image inner;

        [SerializeField] Color normalColor = new Color(0.06f, 0.05f, 0.14f, 1f);
        [SerializeField] Color rightColor = new Color(0.16f, 0.62f, 0.3f, 1f);
        [SerializeField] Color wrongColor = new Color(0.75f, 0.2f, 0.18f, 1f);
        [SerializeField] float flashSeconds = 0.9f;

        public event Action<QuizCard> Picked;

        /// <summary>Which word (index into the quiz's word list) this card stands for.</summary>
        public int WordIndex { get; private set; }

        void Awake()
        {
            button.onClick.AddListener(() => Picked?.Invoke(this));
        }

        public void Show(string text, int wordIndex)
        {
            StopAllCoroutines();
            meaning.text = text;
            WordIndex = wordIndex;
            inner.color = normalColor;
            button.interactable = true;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        public void SetInteractable(bool on) => button.interactable = on;

        /// <summary>Blink the card green or red, then hold that colour.</summary>
        public void Flash(bool correct)
        {
            if (!isActiveAndEnabled) return;
            StopAllCoroutines();
            StartCoroutine(Blink(correct ? rightColor : wrongColor));
        }

        IEnumerator Blink(Color color)
        {
            const int blinks = 3;
            float step = flashSeconds / (blinks * 2);
            for (int i = 0; i < blinks; i++)
            {
                inner.color = color;
                yield return new WaitForSeconds(step);
                inner.color = normalColor;
                yield return new WaitForSeconds(step);
            }
            inner.color = color;
        }

        /// <summary>Editor testing: same as pointing at the card and pulling the trigger.</summary>
        public void SimulatePick() => button.onClick.Invoke();
    }
}
