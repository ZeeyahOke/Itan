using System;
using UnityEngine;

namespace Itan.Detection
{
    /// <summary>
    /// Watches the headset and fires Bowed when the visitor bows their head
    /// past bowAngle and holds it for holdSeconds.
    /// </summary>
    public class GreetingDetector : MonoBehaviour
    {
        [SerializeField] Transform head;

        [Tooltip("How far the head must tilt down, in degrees.")]
        [SerializeField] float bowAngle = 25f;

        [Tooltip("How long the bow must be held, in seconds.")]
        [SerializeField] float holdSeconds = 0.5f;

        public event Action Bowed;

        BowCheck check;
        bool listening;

        void Awake() => check = new BowCheck(bowAngle, holdSeconds);

        public void StartListening()
        {
            check.Reset();
            listening = true;
        }

        public void StopListening() => listening = false;

        void Update()
        {
            if (!listening) return;

            // forward.y is -sin(tilt), so tilt down = -asin(forward.y).
            float pitchDown = -Mathf.Asin(Mathf.Clamp(head.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
            if (check.Update(pitchDown, Time.deltaTime))
                CompleteBow();
        }

        /// <summary>Editor testing without a headset.</summary>
        public void SimulateBow()
        {
            if (listening) CompleteBow();
        }

        void CompleteBow()
        {
            listening = false;
            Debug.Log("[Greeting] Bow detected");
            Bowed?.Invoke();
        }
    }
}
