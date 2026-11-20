using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Itan.Player
{
    /// <summary>
    /// Puts every visitor's eyes at the same spot, facing the fire, whatever
    /// their height, chair or which way they were looking when they put the
    /// headset on. It moves the XR Origin, not the camera, so head tracking
    /// still works normally afterwards.
    /// </summary>
    public class SeatRecenter : MonoBehaviour
    {
        [SerializeField] XROrigin origin;

        [Tooltip("Where the visitor's eyes should be. Its blue (forward) arrow should point at the fire.")]
        [SerializeField] Transform seatEye;

        public void Recenter()
        {
            StartCoroutine(RecenterAfterTracking());
        }

        IEnumerator RecenterAfterTracking()
        {
            // Give the headset a couple of frames to report a real head pose.
            yield return null;
            yield return null;
            Apply();
        }

        /// <summary>Recenter straight away (the headset is already tracking).</summary>
        public void RecenterNow() => Apply();

        void Apply()
        {
            Transform head = origin.Camera.transform;

            // 1. Turn: rotate the rig around the head so the head's flat forward matches the seat's.
            Vector3 headForward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            Vector3 seatForward = Vector3.ProjectOnPlane(seatEye.forward, Vector3.up);
            if (headForward.sqrMagnitude > 0.0001f)
                origin.RotateAroundCameraUsingOriginUp(Vector3.SignedAngle(headForward, seatForward, Vector3.up));

            // 2. Move: shift the rig so the head lands exactly on the seat's eye point.
            origin.MoveCameraToWorldLocation(seatEye.position);

            Debug.Log("[Seat] Recentered view at " + seatEye.position);
        }
    }
}
