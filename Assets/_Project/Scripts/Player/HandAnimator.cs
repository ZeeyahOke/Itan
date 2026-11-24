using UnityEngine;
using UnityEngine.InputSystem;

namespace Itan.Player
{
    /// <summary>
    /// Curls the visitor's hand with the controller: grip closes the middle, ring and little
    /// fingers and the thumb; trigger curls the index finger. (Same hands as the VR Room project:
    /// "Free Pack - VR Hands (Rigged)" by PolyOne Studio, CC BY 4.0, with dark skin.)
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HandAnimator : MonoBehaviour
    {
        static readonly int GripId = Animator.StringToHash("Grip");
        static readonly int TriggerId = Animator.StringToHash("Trigger");

        [SerializeField] InputActionProperty gripAction;
        [SerializeField] InputActionProperty triggerAction;

        Animator animator;

        void Awake() => animator = GetComponent<Animator>();

        void Update()
        {
            animator.SetFloat(GripId, Read(gripAction));
            animator.SetFloat(TriggerId, Read(triggerAction));
        }

        static float Read(InputActionProperty property) => property.action != null ? property.action.ReadValue<float>() : 0f;
    }
}
