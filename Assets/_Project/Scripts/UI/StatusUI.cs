using Itan.Interaction;
using Itan.Story;
using TMPro;
using UnityEngine;

namespace Itan.UI
{
    /// <summary>
    /// Shows progress through the story: "Word 1 of 3" plus the beat title on a
    /// small label above the calabash, and fills the calabash a third per word.
    /// </summary>
    public class StatusUI : MonoBehaviour
    {
        [SerializeField] StoryManager story;
        [SerializeField] WordCardMoment wordMoment;
        [SerializeField] CalabashFill fill;
        [SerializeField] GameObject panel;
        [SerializeField] TextMeshProUGUI wordLabel;
        [SerializeField] TextMeshProUGUI beatLabel;

        int collected;
        int total;

        void Awake()
        {
            total = 0;
            foreach (var beat in story.Beats) if (beat.wordCard != null) total++;
            ResetStatus();
        }

        void OnEnable()
        {
            story.BeatStarted += OnBeatStarted;
            wordMoment.WordCollected += OnWordCollected;
        }

        void OnDisable()
        {
            story.BeatStarted -= OnBeatStarted;
            wordMoment.WordCollected -= OnWordCollected;
        }

        public int Collected => collected;

        void OnBeatStarted(StoryBeat beat, int index)
        {
            panel.SetActive(true);
            wordLabel.text = "Word " + (index + 1) + " of " + total;
            beatLabel.text = beat.title;
        }

        void OnWordCollected(WordCard word, int tries, bool byButton)
        {
            collected++;
            fill.SetFill(total == 0 ? 0f : (float)collected / total);
            wordLabel.text = collected + " of " + total + " words";
        }

        public void ResetStatus()
        {
            collected = 0;
            fill.ResetFill();
            panel.SetActive(false);
        }
    }
}
