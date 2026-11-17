using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Itan.Interaction
{
    /// <summary>
    /// The visitor's calabash. It waits on its stand until offered; the first
    /// time it is grabbed it settles into the visitor's lap and can't be
    /// grabbed or dropped again for the rest of the visit.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class Calabash : MonoBehaviour
    {
        [Tooltip("Empty transform on the player's lap where the calabash rests.")]
        [SerializeField] Transform lapPoint;

        public event Action Snapped;
        public bool IsInLap { get; private set; }

        XRGrabInteractable grab;
        Rigidbody body;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            grab.selectEntered.AddListener(OnGrabbed);
        }

        void OnDestroy() => grab.selectEntered.RemoveListener(OnGrabbed);

        /// <summary>Show the calabash on its stand, ready to be picked up.</summary>
        public void Offer()
        {
            gameObject.SetActive(true);
            grab.enabled = true;
        }

        void OnGrabbed(SelectEnterEventArgs args)
        {
            // Let XRI finish the grab this frame, then take it out of the hand.
            StartCoroutine(SnapNextFrame());
        }

        IEnumerator SnapNextFrame()
        {
            yield return null;
            SnapToLap();
        }

        public void SnapToLap()
        {
            if (IsInLap) return;

            if (grab.isSelected)
                grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
            grab.enabled = false;           // can't be grabbed (or dropped) again

            body.isKinematic = true;
            transform.SetParent(lapPoint, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            IsInLap = true;
            Debug.Log("[Calabash] Snapped to lap");
            Snapped?.Invoke();
        }
    }
}
