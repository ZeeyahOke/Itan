using TMPro;
using UnityEngine;

namespace Itan.UI
{
    /// <summary>
    /// World-space English subtitle under the elder. Hidden when nobody is talking.
    /// </summary>
    public class SubtitlePanel : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TextMeshProUGUI text;

        void Awake() => Hide();

        public void Show(string line)
        {
            text.text = line;
            panel.SetActive(true);
        }

        public void Hide() => panel.SetActive(false);
    }
}
