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

        // Where it waits on its stand, so the next visitor finds it there again.
        bool initialised;
        Transform homeParent;
        Vector3 homePosition;
        Quaternion homeRotation;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            grab.selectEntered.AddListener(OnGrabbed);

            homeParent = transform.parent;
            homePosition = transform.position;
            homeRotation = transform.rotation;
            initialised = true;
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

        /// <summary>Back on the stand, hidden and grabbable again, for the next visitor.</summary>
        public void ResetForNextVisitor()
        {
            if (!initialised) return; // never offered, so nothing moved
            StopAllCoroutines();
            transform.SetParent(homeParent, true);
            transform.SetPositionAndRotation(homePosition, homeRotation);
            grab.enabled = true;
            IsInLap = false;
            gameObject.SetActive(false);
        }

        public void SnapToLap()
        {
            if (IsInLap) return;
            // If it was never offered (e.g. the step was skipped), wake it up first so Awake runs.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

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
