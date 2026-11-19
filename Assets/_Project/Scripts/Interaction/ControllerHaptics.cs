using Itan.Story;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace Itan.Interaction
{
    /// <summary>
    /// Short controller vibrations: a light tap when any button is pressed
    /// (menu, fallback, quiz cards) and a stronger pulse when a word card
    /// lands in the calabash.
    /// </summary>
    public class ControllerHaptics : MonoBehaviour
    {
        [SerializeField] WordCardMoment wordMoment;

        [Header("Button press")]
        [SerializeField, Range(0f, 1f)] float tapAmplitude = 0.35f;
        [SerializeField] float tapSeconds = 0.06f;

        [Header("Word into calabash")]
        [SerializeField, Range(0f, 1f)] float collectAmplitude = 0.7f;
        [SerializeField] float collectSeconds = 0.18f;

        HapticImpulsePlayer[] players;

        void Start()
        {
            players = FindObjectsByType<HapticImpulsePlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log("[Haptics] Found " + players.Length + " controller haptic player(s)");

            // Every button in the scene gives a little tap when pressed.
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                button.onClick.AddListener(Tap);

            if (wordMoment != null) wordMoment.WordCollected += (word, tries, byButton) => Collect();
        }

        public void Tap() => Pulse(tapAmplitude, tapSeconds);

        public void Collect() => Pulse(collectAmplitude, collectSeconds);

        public void Pulse(float amplitude, float seconds)
        {
            if (players == null) return;
            foreach (var p in players)
                if (p != null && p.isActiveAndEnabled) p.SendHapticImpulse(amplitude, seconds);
        }
    }
}
