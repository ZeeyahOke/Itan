using System;
using System.Collections.Generic;
using System.Text;
using Itan.Story;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Itan.UI
{
    /// <summary>
    /// End-of-visit panel: the words learned, the quiz score and a Finish button
    /// that hands the headset over to the next visitor.
    /// </summary>
    public class SummaryPanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI wordsText;
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] Button finishButton;

        public event Action FinishPressed;

        void Awake()
        {
            finishButton.onClick.AddListener(() => FinishPressed?.Invoke());
            Hide();
        }

        public void Show(IEnumerable<WordCard> words, int score, int total)
        {
            var sb = new StringBuilder();
            foreach (var w in words)
                sb.AppendLine("<b>" + w.yorubaWord + "</b>   " + w.englishMeaning);
            wordsText.text = sb.ToString().TrimEnd();
            scoreText.text = "Quiz: " + score + " of " + total;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>Editor testing: same as pressing Finish with the ray.</summary>
        public void SimulateFinish() => finishButton.onClick.Invoke();
    }
}
