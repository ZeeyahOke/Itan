using UnityEngine;

namespace Itan.Player
{
    /// <summary>
    /// Holds everything that belongs to the visitor rather than to a place: subtitles,
    /// the word card, the listening prompt, the status label and the lap (with the
    /// calabash). At the fire it sits at the seat. When the visitor climbs the rope or
    /// stands on the cloud, it is carried along so all of that stays in view.
    /// </summary>
    public class VisitorSpace : MonoBehaviour
    {
        public static VisitorSpace Instance { get; private set; }

        [Tooltip("The visitor's eye point at the fire. Everything here is laid out around it.")]
        [SerializeField] Transform seatEye;

        void Awake() => Instance = this;
        void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>Carry the visitor's things so the seat's eye point lands on <paramref name="eye"/>, facing <paramref name="forward"/>.</summary>
        public void MoveTo(Vector3 eye, Vector3 forward)
        {
            float yaw = Vector3.SignedAngle(Vector3.ProjectOnPlane(seatEye.forward, Vector3.up), Vector3.ProjectOnPlane(forward, Vector3.up), Vector3.up);
            var rot = Quaternion.Euler(0f, yaw, 0f);
            // seatEye is not a child, so map it by hand: new = eye + rot * (p - seatEye).
            transform.SetPositionAndRotation(eye - rot * seatEye.position, rot);
        }

        /// <summary>Back to the seat by the fire.</summary>
        public void ResetToSeat() => transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
