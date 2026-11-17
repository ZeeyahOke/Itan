using Itan.Narration;
using UnityEngine;

namespace Itan.Characters
{
    /// <summary>
    /// Switches the elder between Idle and Talk while narration plays.
    /// Works with the greybox clips now and Mixamo clips later, as long as
    /// the Animator has a bool called "Talking".
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ElderAnimator : MonoBehaviour
    {
        static readonly int Talking = Animator.StringToHash("Talking");

        [SerializeField] NarrationManager narration;

        Animator animator;

        void Awake() => animator = GetComponent<Animator>();

        void OnEnable()
        {
            if (narration != null) narration.SpeakingChanged += OnSpeakingChanged;
        }

        void OnDisable()
        {
            if (narration != null) narration.SpeakingChanged -= OnSpeakingChanged;
        }

        void OnSpeakingChanged(bool speaking) => animator.SetBool(Talking, speaking);
    }
}
